using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BetfairNG.Data;

namespace BetfairNG
{
    internal class Poller : IDisposable
    {
        private readonly IDisposable _poller;
        internal DateTime LatestDataRequestStart = DateTime.Now;
        internal DateTime LatestDataRequestFinish = DateTime.Now;

        public Poller(IDisposable poller)
        {
            this._poller = poller;
        }
        public void Dispose()
        {
            _poller.Dispose();
        }
    }

    public class MarketListenerMultiPeriod : IDisposable
    {
        private readonly PriceProjection _priceProjection;
        private readonly BetfairClient _client;

        private readonly object _lockObj = new object();

        private readonly ConcurrentDictionary<string, IObservable<MarketBook>> _markets =
            new ConcurrentDictionary<string, IObservable<MarketBook>>();

        private readonly ConcurrentDictionary<string, IObserver<MarketBook>> _observers =
            new ConcurrentDictionary<string, IObserver<MarketBook>>();

        private readonly ConcurrentDictionary<double, ConcurrentDictionary<string, bool>> _marketPollInterval =
            new ConcurrentDictionary<double, ConcurrentDictionary<string, bool>>();

        private readonly ConcurrentDictionary<double, Poller> _polling =
            new ConcurrentDictionary<double, Poller>();

        private MarketListenerMultiPeriod(BetfairClient client,
            PriceProjection priceProjection)
        {
            _client = client;
            _priceProjection = priceProjection;
        }

        public static MarketListenerMultiPeriod Create(BetfairClient client,
            PriceProjection priceProjection)
        {
            return new MarketListenerMultiPeriod(client, priceProjection);
        }

        public IObservable<Runner> SubscribeRunner(string marketId, long selectionId, long pollinterval)
        {
            var marketTicks = SubscribeMarketBook(marketId, pollinterval);

            var observable = Observable.Create<Runner>(
              (IObserver<Runner> observer) =>
              {
                  var subscription = marketTicks.Subscribe(tick =>
                  {
                      var runner = tick.Runners.First(c => c.SelectionId == selectionId);
                      // attach the book
                      runner.MarketBook = tick;
                      observer.OnNext(runner);
                  });

                  return Disposable.Create(() => subscription.Dispose());
              })
              .Publish()
              .RefCount();

            return observable;
        }

        public IObservable<MarketBook> SubscribeMarketBook(string marketId, double pollIntervalInSeconds)
        {
            IObservable<MarketBook> market;
            if (_markets.TryGetValue(marketId, out market))
                return market;

            SetupMarketPolling(marketId, pollIntervalInSeconds);

            var observable = Observable.Create<MarketBook>(
               observer =>
               {
                   _observers.AddOrUpdate(marketId, observer, (key, existingVal) => existingVal);
                   return Disposable.Create(() =>
                   {
                       IObserver<MarketBook> ob;
                       IObservable<MarketBook> o;
                       _markets.TryRemove(marketId, out o);
                       _observers.TryRemove(marketId, out ob);
                       
                       CleanUpPolling(marketId);
                   });
               })
               .Publish()
               .RefCount();

            _markets.AddOrUpdate(marketId, observable, (key, existingVal) => existingVal);
            return observable;
        }


        private void DoWork(double pollinterval)
        {
            ConcurrentDictionary<string, bool> bag;
            if (!_marketPollInterval.TryGetValue(pollinterval, out bag)) return;
            
            var book = _client.ListMarketBook(bag.Keys, _priceProjection).Result;

            if (book.HasError)
            {
                foreach (var observer in _observers.Where(k => bag.Keys.Contains(k.Key)))
                {
                    observer.Value.OnError(book.Error);
                }
                return;
            }

            // we may have fresher data than the response to this pollinterval request
            Poller p;
            if (!_polling.TryGetValue(pollinterval, out p)) return;

            if (book.RequestStart < p.LatestDataRequestStart && book.LastByte > p.LatestDataRequestFinish)
                return;

            lock (_lockObj)
            {
                p.LatestDataRequestStart = book.RequestStart;
                p.LatestDataRequestFinish = book.LastByte;
            }

            foreach (var market in book.Response)
            {
                IObserver<MarketBook> o;
                if (!_observers.TryGetValue(market.MarketId, out o)) continue;

                // check to see if the market is finished
                if (market.Status == MarketStatus.CLOSED ||
                    market.Status == MarketStatus.INACTIVE)
                    o.OnCompleted();
                else
                    o.OnNext(market);
            }

        }

        public void UpdatePollInterval(string marketId, double newPollIntervalInSeconds)
        {
            if (!_markets.Keys.Contains(marketId)) return;

            lock (_lockObj)
            {
                // First, remove the existing entry
                CleanUpPolling(marketId);
                // Now put this marketId into the new interval
                SetupMarketPolling(marketId, newPollIntervalInSeconds);
            }
        }

        private void SetupMarketPolling(string marketId, double pollIntervalInSeconds)
        {
            // Keep the poll interval reasonable...
            if (pollIntervalInSeconds < 0.15) pollIntervalInSeconds = 0.15;

            ConcurrentDictionary<string, bool> marketIdsForPollInterval;
            if (_marketPollInterval.TryGetValue(pollIntervalInSeconds, out marketIdsForPollInterval))
            {
                marketIdsForPollInterval.TryAdd(marketId, false);
            }
            else
            {
                marketIdsForPollInterval = new ConcurrentDictionary<string, bool>();
                marketIdsForPollInterval.TryAdd(marketId, false);

                _marketPollInterval.TryAdd(pollIntervalInSeconds, marketIdsForPollInterval);
                _polling.TryAdd(pollIntervalInSeconds, new Poller(
                    Observable.Interval(TimeSpan.FromSeconds(pollIntervalInSeconds), NewThreadScheduler.Default)
                        .Subscribe(
                            onNext: l => DoWork(pollIntervalInSeconds)
                            //, onCompleted: TODO: do I need some clean up here?
                        )));
            }
        }

        private void CleanUpPolling(string marketId)
        {
            // Find the interval that the market is now running under
            var interval = _marketPollInterval.First(search => search.Value.Keys.Contains(marketId)).Key;

            ConcurrentDictionary<string, bool> mpi;
            if (_marketPollInterval.TryGetValue(interval, out mpi))
            {
                bool pi;
                mpi.TryRemove(marketId, out pi);

                if (!mpi.IsEmpty) return;
                // All the markets have gone for this interval, so clean the interval + polling up as well
                ConcurrentDictionary<string, bool> pis;
                if (_marketPollInterval.TryRemove(interval, out pis))
                {
                    Poller poll;
                    if (_polling.TryRemove(interval, out poll))
                    {
                        poll.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var poll in _polling)
            {
                if (poll.Value != null) poll.Value.Dispose();
            }
        }
    }
}

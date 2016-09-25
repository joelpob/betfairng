using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BetfairNG.Data;

namespace BetfairNG
{
    public class MarketListenerPeriodic : IDisposable
    {
        private readonly PriceProjection _priceProjection;
        private readonly BetfairClient _client;

        private DateTime _latestDataRequestStart = DateTime.Now;
        private DateTime _latestDataRequestFinish = DateTime.Now;

        private readonly object _lockObj = new object();

        private readonly ConcurrentDictionary<string, IObservable<MarketBook>> _markets =
            new ConcurrentDictionary<string, IObservable<MarketBook>>();

        private readonly ConcurrentDictionary<string, IObserver<MarketBook>> _observers =
            new ConcurrentDictionary<string, IObserver<MarketBook>>();

        private readonly IDisposable _polling;

        private MarketListenerPeriodic(BetfairClient client,
            PriceProjection priceProjection,
            double periodInSec)
        {
            _client = client;
            _priceProjection = priceProjection;

            _polling = Observable.Interval(TimeSpan.FromSeconds(periodInSec),
                                          NewThreadScheduler.Default).Subscribe(l => DoWork());
        }

        public static MarketListenerPeriodic Create(BetfairClient client,
            PriceProjection priceProjection,
            double periodInSec)
        {
            return new MarketListenerPeriodic(client, priceProjection, periodInSec);
        }

        public IObservable<Runner> SubscribeRunner(string marketId, long selectionId)
        {
            var marketTicks = SubscribeMarketBook(marketId);

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

        public IObservable<MarketBook> SubscribeMarketBook(string marketId)
        {
            IObservable<MarketBook> market;
            if (_markets.TryGetValue(marketId, out market))
                return market;

            var observable = Observable.Create<MarketBook>(
               (IObserver<MarketBook> observer) =>
               {
                   _observers.AddOrUpdate(marketId, observer, (key, existingVal) => existingVal);
                   return Disposable.Create(() =>
                       {
                           IObserver<MarketBook> ob;
                           IObservable<MarketBook> o;
                           _markets.TryRemove(marketId, out o);
                           _observers.TryRemove(marketId, out ob);
                       });
               })
               .Publish()
               .RefCount();

            _markets.AddOrUpdate(marketId, observable, (key, existingVal) => existingVal);
            return observable;
        }


        private void DoWork()
        {
            var book = _client.ListMarketBook(_markets.Keys.ToList(), this._priceProjection).Result;

            if (book.HasError)
            {
                foreach (var observer in _observers)
                    observer.Value.OnError(book.Error);
                return;
            }

            // we may have fresher data than the response to this request
            if (book.RequestStart < _latestDataRequestStart && book.LastByte > _latestDataRequestFinish)
                return;

            lock (_lockObj)
            {
                _latestDataRequestStart = book.RequestStart;
                _latestDataRequestFinish = book.LastByte;
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

        public void Dispose()
        {
            _polling?.Dispose();
        }
    }
}

﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetfairNG.Data;
using MoreLinq;

namespace BetfairNG
{
    public class MarketListener
    {
        private static MarketListener listener = null;
        private int connectionCount;
        private PriceProjection priceProjection;        
        private BetfairClient client;
        private static DateTime lastRequestStart;

        private static DateTime latestDataRequestStart = DateTime.Now;
        private static DateTime latestDataRequestFinish = DateTime.Now;

        private static object lockObj = new object();

        private ConcurrentDictionary<string, IObservable<MarketBook>> markets =
            new ConcurrentDictionary<string, IObservable<MarketBook>>();

        private ConcurrentDictionary<string, IObserver<MarketBook>> observers =
            new ConcurrentDictionary<string, IObserver<MarketBook>>();

        private MarketListener(BetfairClient client, 
            PriceProjection priceProjection, 
            int connectionCount)
        {
            this.client = client;
            this.priceProjection = priceProjection;
            this.connectionCount = connectionCount;
            Task.Run(() => PollMarketBooks());
        }

        public static MarketListener Create(BetfairClient client, 
            PriceProjection priceProjection, 
            int connectionCount)
        {
            if (listener == null)
                listener = new MarketListener(client, priceProjection, connectionCount);

            return listener;
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
            if (markets.TryGetValue(marketId, out market))
                return market;

            var observable = Observable.Create<MarketBook>(
               (IObserver<MarketBook> observer) =>
               {
                   observers.AddOrUpdate(marketId, observer, (key, existingVal) => existingVal);
                   return Disposable.Create(() =>
                       {
                           IObserver<MarketBook> ob;
                           IObservable<MarketBook> o;
                           markets.TryRemove(marketId, out o);
                           observers.TryRemove(marketId, out ob);
                       });
               })
               .Publish()
               .RefCount();

            markets.AddOrUpdate(marketId, observable, (key, existingVal) => existingVal);
            return observable;
        }

        // TODO:// replace this with the Rx scheduler 
        private void PollMarketBooks()
        {
            for (int i = 0; i < connectionCount;i++)
            {
                Task.Run(() =>
                    {
                        while (true)
                        {
                            if (markets.Count > 0)
                            {
                                // TODO:// look at spinwait or signalling instead of this
                                while (connectionCount > 1 && DateTime.Now.Subtract(lastRequestStart).TotalMilliseconds < (1000 / connectionCount))
                                {
                                    int waitMs = (1000 / connectionCount) - (int)DateTime.Now.Subtract(lastRequestStart).TotalMilliseconds;
                                    Thread.Sleep(waitMs > 0 ? waitMs : 0);
                                }

                                lock (lockObj)
                                    lastRequestStart = DateTime.Now;
                                                                
                                var books = client.ListMarketBook(markets.Keys.ToList(), this.priceProjection).Result;
                                var bookInError = books.FirstOrDefault(b => b.HasError);

                                if (bookInError == null)
                                {
                                    // we may have fresher data than the response to this request
                                    if (books.Any(b => b.RequestStart < latestDataRequestStart && b.LastByte > latestDataRequestFinish))
                                        continue;
                                    else
                                    {
                                        lock (lockObj)
                                        {
                                            var latestBook = books.MinBy(b => b.RequestStart);
                                            latestDataRequestStart = latestBook.RequestStart;
                                            latestDataRequestFinish = latestBook.LastByte;
                                        }
                                    }
                                    foreach (var book in books)
                                    {
                                        foreach (var market in book.Response)
                                        {
                                            IObserver<MarketBook> o;
                                            if (observers.TryGetValue(market.MarketId, out o))
                                            {
                                                // check to see if the market is finished
                                                if (market.Status == MarketStatus.CLOSED ||
                                                    market.Status == MarketStatus.INACTIVE ||
                                                    market.Status == MarketStatus.SUSPENDED)
                                                    o.OnCompleted();
                                                else
                                                    o.OnNext(market);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var observer in observers)
                                        observer.Value.OnError(books.First(x => x.HasError).Error);
                                }
                            }
                            else
                                // TODO:// will die with rx scheduler
                                Thread.Sleep(500);
                        }
                    });
                Thread.Sleep(1000 / connectionCount);
            }
        }
    }
}

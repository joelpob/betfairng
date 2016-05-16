﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using BetfairNG.Data;
using System.Reactive.Disposables;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace BetfairNG
{
    public class MarketListener
    {
        private static MarketListener listener = null;
        private int connectionCount;
        private int samplePeriod;
        private int sampleFrequency;
        private bool marketAdded = false;
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
            int connectionCount, int samplePeriod)
        {
            this.client = client;
            this.priceProjection = priceProjection;
            this.connectionCount = connectionCount;
            this.samplePeriod = samplePeriod;
            if (samplePeriod >= 1000)
                this.sampleFrequency = samplePeriod/1000;
            else
                this.sampleFrequency = samplePeriod/100;
            Task.Run(() => PollMarketBooks());
        }

        public static MarketListener Create(BetfairClient client, 
            PriceProjection priceProjection, 
            int connectionCount, int samplePeriod = 0)
        {
            if (listener == null)
                listener = new MarketListener(client, priceProjection, connectionCount, samplePeriod);

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
            marketAdded = true;
            return observable;
        }

        // TODO:// replace this with the Rx scheduler 
        private void PollMarketBooks()
        {
            for (int i = 0; i < connectionCount;i++)
            {
                Task.Run(async () =>
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

                                var stopWatch = new Stopwatch();
                                stopWatch.Start();

                                lock (lockObj)
                                    lastRequestStart = DateTime.Now;

                                var book = client.ListMarketBook(markets.Keys.ToList(), this.priceProjection).Result;

                                if (!book.HasError)
                                {
                                    // we may have fresher data than the response to this request
                                    if (book.RequestStart < latestDataRequestStart && book.LastByte > latestDataRequestFinish)
                                        continue;
                                    else
                                    {
                                        lock (lockObj)
                                        {
                                            latestDataRequestStart = book.RequestStart;
                                            latestDataRequestFinish = book.LastByte;
                                        }
                                    }

                                    foreach (var market in book.Response)
                                    {
                                        IObserver<MarketBook> o;
                                        if (observers.TryGetValue(market.MarketId, out o))
                                        {
                                            // check to see if the market is finished
                                            if (market.Status == MarketStatus.CLOSED ||
                                                market.Status == MarketStatus.INACTIVE)
                                                o.OnCompleted();
                                            else
                                                o.OnNext(market);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var observer in observers)
                                        observer.Value.OnError(book.Error);
                                }
                                //var j = 0;

                                //int sleepTime = samplePeriod - (int)DateTime.Now.Subtract(lastRequestStart).TotalMilliseconds;

                                while (stopWatch.ElapsedMilliseconds < samplePeriod && !marketAdded)
                                {
                                    await Task.Delay(sampleFrequency);
                                    //j = j + sampleFrequency;
                                }
                                marketAdded = false;
                                //var x = stopWatch.ElapsedMilliseconds;
                            }
                            else
                                // TODO:// will die with rx scheduler
                                await Task.Delay(500);
                        }
                    });
                Thread.Sleep(1000 / connectionCount);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Betfair.ESAClient;
using Betfair.ESASwagger.Model;
using Betfair.ESAClient.Auth;
using Betfair.ESAClient.Cache;
using Betfair.ESAClient.Protocol;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace BetfairNG
{
    /// <summary>
    /// Streaming Betfair Client with caching
    /// </summary>
    public class StreamingBetfairClient : IChangeMessageHandler
    {
        private string streamEndPointHostName;
        private string appKey;
        private Client networkClient;
        
        private Action preNetworkRequest;
        private static TraceSource trace = new TraceSource("StreamingBetfairClient");

        private ConcurrentDictionary<string, IObservable<MarketSnap>> marketsObservables =
            new ConcurrentDictionary<string, IObservable<MarketSnap>>();
        private ConcurrentDictionary<string, IObserver<MarketSnap>> marketObservers =
            new ConcurrentDictionary<string, IObserver<MarketSnap>>();

        private ConcurrentDictionary<string, IObservable<OrderMarketSnap>> orderObservables =
            new ConcurrentDictionary<string, IObservable<OrderMarketSnap>>();
        private ConcurrentDictionary<string, IObserver<OrderMarketSnap>> orderObservers =
            new ConcurrentDictionary<string, IObserver<OrderMarketSnap>>();

        MarketCache marketCache = new MarketCache();
        OrderCache orderCache = new OrderCache();

        public StreamingBetfairClient(
            string streamEndPointHostName,
            string appKey,
            Action preNetworkRequest = null)
        {
            if (string.IsNullOrWhiteSpace(streamEndPointHostName)) throw new ArgumentException("streamEndPointHostName");
            if (string.IsNullOrWhiteSpace(appKey)) throw new ArgumentException("appKey");

            this.streamEndPointHostName = streamEndPointHostName;
            this.appKey = appKey;
            this.preNetworkRequest = preNetworkRequest;

            this.marketCache.MarketChanged += MarketCache_MarketChanged;
            this.orderCache.OrderMarketChanged += OrderCache_OrderMarketChanged;
        }

        public bool Login(string username, string password, string ssoHostName = "identitysso.betfair.com")
        {
            AppKeyAndSessionProvider authProvider = new AppKeyAndSessionProvider(ssoHostName, appKey, username, password);
            networkClient = new Client(streamEndPointHostName, 443, authProvider);
            networkClient.ChangeHandler = this;
            networkClient.Start();

            return true;
        }

        public IObservable<MarketSnap> SubscribeMarket(string marketId)
        {
            MarketFilter filter = new MarketFilter { MarketIds = new List<string>() { marketId } };
            MarketSubscriptionMessage message = new MarketSubscriptionMessage() { MarketFilter = filter };
            return SubscribeMarket(marketId, message);
        }

        public IObservable<MarketSnap> SubscribeMarket(string marketId, MarketSubscriptionMessage message)
        {
            networkClient.Start();

            IObservable<MarketSnap> market;
            if (marketsObservables.TryGetValue(marketId, out market))
            {
                networkClient.MarketSubscription(message);
                return market;
            }

            var observable = Observable.Create<MarketSnap>(
               (IObserver<MarketSnap> observer) =>
               {
                   marketObservers.AddOrUpdate(marketId, observer, (key, existingVal) => existingVal);
                   return Disposable.Create(() =>
                   {
                       IObserver<MarketSnap> ob;
                       IObservable<MarketSnap> o;
                       marketsObservables.TryRemove(marketId, out o);
                       marketObservers.TryRemove(marketId, out ob);
                   });
               })
               .Publish()
               .RefCount();

            marketsObservables.AddOrUpdate(marketId, observable, (key, existingVal) => existingVal);

            // TODO:// race? 
            networkClient.MarketSubscription(message);
            return observable;
        }


        public IObservable<OrderMarketSnap> SubscribeOrders(string marketId)
        {
            OrderSubscriptionMessage orderSubscription = new OrderSubscriptionMessage();
            return SubscribeOrders(marketId, orderSubscription);
        }

        public IObservable<OrderMarketSnap> SubscribeOrders(string marketId, OrderSubscriptionMessage orderSubscription)
        {
            networkClient.Start();

            IObservable<OrderMarketSnap> orderObservable;
            if (orderObservables.TryGetValue(marketId, out orderObservable))
            {
                networkClient.OrderSubscription(orderSubscription);
                return orderObservable;
            }

            var observable = Observable.Create<OrderMarketSnap>(
               (IObserver<OrderMarketSnap> observer) =>
               {
                   orderObservers.AddOrUpdate(marketId, observer, (key, existingVal) => existingVal);

                   return Disposable.Create(() =>
                   {
                       IObserver<OrderMarketSnap> ob;
                       IObservable<OrderMarketSnap> o;
                       orderObservables.TryRemove(marketId, out o);
                       orderObservers.TryRemove(marketId, out ob);
                   });
               })
               .Publish()
               .RefCount();

            orderObservables.AddOrUpdate(marketId, observable, (key, existingVal) => existingVal);

            // TODO:// race? 
            networkClient.OrderSubscription(orderSubscription);
            return observable;
        }

        public long? ConflatMs
        {
            get
            {
                return networkClient.ConflateMs;
            }
            set
            {
                networkClient.ConflateMs = value;
            }
        }

        public ConnectionStatus Status
        {
            get
            {
                return networkClient.Status;
            }
        }

        public void OnOrderChange(ChangeMessage<OrderMarketChange> changeMessage)
        {
            orderCache.OnOrderChange(changeMessage);
        }

        public void OnMarketChange(ChangeMessage<MarketChange> changeMessage)
        {
            marketCache.OnMarketChange(changeMessage);
        }

        public void OnErrorStatusNotification(StatusMessage message)
        {
            throw new NotImplementedException();
        }

        private void MarketCache_MarketChanged(object sender, MarketChangedEventArgs e)
        {
            IObserver<MarketSnap> o;
            if (marketObservers.TryGetValue(e.Market.MarketId, out o))
            {
                // check to see if the market is finished
                if (e.Market.IsClosed)
                    o.OnCompleted();
                else
                    o.OnNext(e.Snap);
            }
        }

        private void OrderCache_OrderMarketChanged(object sender, OrderMarketChangedEventArgs e)
        {
            IObserver<OrderMarketSnap> o;
            if (orderObservers.TryGetValue(e.Snap.MarketId, out o))
            {
                // check to see if the market is finished
                if (e.Snap.IsClosed)
                    o.OnCompleted();
                else
                    o.OnNext(e.Snap);
            }
        }
    }
}

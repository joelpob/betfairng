using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using BetfairNG;
using BetfairNG.Data;

public class PeriodicExample : IDisposable
{
    private readonly ConcurrentQueue<MarketCatalogue> _markets = new ConcurrentQueue<MarketCatalogue>();
    private readonly MarketListenerPeriodic _marketListener;

    private IDisposable _marketSubscription;
    public bool IsBlocking { get { return false; } }

    public PeriodicExample(BetfairClient client, double pollIntervalInSeconds)
    {
        var betfairClient = client;

        var marketCatalogues = betfairClient.ListMarketCatalogue(
            BFHelpers.HorseRaceFilter("GB"),
            BFHelpers.HorseRaceProjection(),
            MarketSort.FIRST_TO_START,
            25).Result.Response;

        marketCatalogues.ForEach(c =>
        {
            _markets.Enqueue(c);
        });

        _marketListener = MarketListenerPeriodic.Create(betfairClient
                                                        , BFHelpers.HorseRacePriceProjection()
                                                        ,pollIntervalInSeconds);
    }

    public void Go()
    {
        MarketCatalogue marketCatalogue;
        _markets.TryDequeue(out marketCatalogue);

        _marketSubscription = _marketListener.SubscribeMarketBook(marketCatalogue.MarketId)
            .SubscribeOn(Scheduler.Default)
            .Subscribe(
                tick =>
                {
                    Console.WriteLine(BFHelpers.MarketBookConsole(marketCatalogue, tick, marketCatalogue.Runners));
                },
                () =>
                {
                    Console.WriteLine("Market finished");
                });
    }

    public void Dispose()
    {
        _marketSubscription.Dispose();
    }
}

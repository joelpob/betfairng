using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using BetfairNG;
using BetfairNG.Data;

public class MultiPeriodExample : IDisposable
{
    private readonly BetfairClient _client;
    private readonly ConcurrentQueue<MarketCatalogue> _markets = new ConcurrentQueue<MarketCatalogue>();

    private IDisposable _marketSubscription1;
    private IDisposable _marketSubscription2;

    public MultiPeriodExample(BetfairClient client)
    {
        _client = client;
    }

    public bool IsBlocking => false;

    public void Go()
    {
        var marketCatalogues = _client.ListMarketCatalogue(
            BFHelpers.HorseRaceFilter("GB"),
            BFHelpers.HorseRaceProjection(),
            MarketSort.FIRST_TO_START,
            25).Result.Response;

        marketCatalogues.ForEach(c =>
        {
            _markets.Enqueue(c);
            Console.WriteLine(c.MarketName);
        });
        Console.WriteLine();

        var marketListener = MarketListenerMultiPeriod.Create(_client, BFHelpers.HorseRacePriceProjection());

        MarketCatalogue marketCatalogue1;
        MarketCatalogue marketCatalogue2;
        _markets.TryDequeue(out marketCatalogue1);
        _markets.TryDequeue(out marketCatalogue2);

        // Red, every 1 second
        _marketSubscription1 = marketListener.SubscribeMarketBook(marketCatalogue1.MarketId, 1)
            .SubscribeOn(Scheduler.Default)
            .Subscribe(
                marketBook =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(BFHelpers.MarketBookConsole(marketCatalogue1, marketBook, marketCatalogue1.Runners));
                    Console.WriteLine();
                },
                () =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Market finished");
                }
            );

        // Blue, every 2.5 second
        _marketSubscription2 = marketListener.SubscribeMarketBook(marketCatalogue2.MarketId, 2.5)
            .SubscribeOn(Scheduler.Default)
            .Subscribe(
                marketBook =>
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(BFHelpers.MarketBookConsole(marketCatalogue2, marketBook, marketCatalogue2.Runners));
                    Console.WriteLine();
                },
                () =>
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Market finished");
                }
            );
    }

    public void Dispose()
    {
        _marketSubscription1?.Dispose();
        _marketSubscription2?.Dispose();
    }
}


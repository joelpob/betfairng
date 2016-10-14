using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using BetfairNG;
using BetfairNG.Data;

public class StreamingExample 
{
    private readonly ConcurrentQueue<MarketCatalogue> _markets = new ConcurrentQueue<MarketCatalogue>();
    private readonly StreamingBetfairClient _streamingClient;
    private readonly BetfairClient _client;

    public StreamingExample(BetfairClient client, StreamingBetfairClient streamingClient)
    {
        _client = client;
        _streamingClient = streamingClient;
    }

    public bool IsBlocking => true;

    public void Go()
    {
        // find all the upcoming UK horse races (EventTypeId 7)
        var marketFilter = new MarketFilter();
        marketFilter.EventTypeIds = new HashSet<string>() { "7" };
        marketFilter.MarketStartTime = new TimeRange()
        {
            From = DateTime.Now,
            To = DateTime.Now.AddDays(2)
        };
        marketFilter.MarketTypeCodes = new HashSet<String>() { "WIN" };

        Console.WriteLine("BetfairClient.ListEvents()");
        var events = _client.ListEvents(marketFilter).Result;
        if (events.HasError)
            throw new ApplicationException();
        var firstEvent = events.Response.First();
        Console.WriteLine("First Event {0} {1}", firstEvent.Event.Id, firstEvent.Event.Name);

        var marketCatalogues = _client.ListMarketCatalogue(
          BFHelpers.HorseRaceFilter(),
          BFHelpers.HorseRaceProjection(),
          MarketSort.FIRST_TO_START,
          25).Result.Response;

        marketCatalogues.ForEach(c =>
        {
            _markets.Enqueue(c);
            Console.WriteLine(c.MarketName);
        });
        Console.WriteLine();

        while (_markets.Count > 0)
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            MarketCatalogue marketCatalogue;
            _markets.TryDequeue(out marketCatalogue);

            var marketSubscription = _streamingClient.SubscribeMarket(marketCatalogue.MarketId)
                .SubscribeOn(Scheduler.Default)
                .Subscribe(
                tick =>
                {
                    Console.WriteLine(BFHelpers.MarketSnapConsole(tick, marketCatalogue.Runners));
                },
                () =>
                {
                    Console.WriteLine("Market finished");
                    waitHandle.Set();
                });

            waitHandle.WaitOne();
            marketSubscription.Dispose();
        }
    }
}
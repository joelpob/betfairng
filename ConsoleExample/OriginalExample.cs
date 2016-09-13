using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using BetfairNG;
using BetfairNG.Data;

public class OriginalExample
{
    private readonly BetfairClient _client;
    private readonly ConcurrentQueue<MarketCatalogue> _markets = new ConcurrentQueue<MarketCatalogue>();

    public OriginalExample(BetfairClient client)
    {
        _client = client;
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

        Console.WriteLine("BetfairClient.ListTimeRanges()");
        var timeRanges = _client.ListTimeRanges(marketFilter, TimeGranularity.HOURS).Result;
        if (timeRanges.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.ListCurrentOrders()");
        var currentOrders = _client.ListCurrentOrders().Result;
        if (currentOrders.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.ListVenues()");
        var venues = _client.ListVenues(marketFilter).Result;
        if (venues.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.GetAccountDetails()");
        var accountDetails = _client.GetAccountDetails().Result;
        if (accountDetails.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.GetAccountStatement()");
        var accountStatement = _client.GetAccountStatement().Result;
        if (accountStatement.HasError)
            throw new ApplicationException();

        Console.Write("BetfairClient.GetAccountFunds() ");
        var acc = _client.GetAccountFunds(Wallet.UK).Result;
        if (acc.HasError)
            throw new ApplicationException();
        Console.WriteLine(acc.Response.AvailableToBetBalance);

        Console.WriteLine("BetfairClient.ListClearedOrders()");
        var clearedOrders = _client.ListClearedOrders(BetStatus.SETTLED).Result;
        if (clearedOrders.HasError)
            throw new ApplicationException();

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

        Console.WriteLine("BetfairClient.ListRaceDetails()");
        var firstMarket = marketCatalogues.First();
        var raceDetails = _client.ListRaceDetails(new HashSet<string>() { firstMarket.Event.Id }).Result;
        Console.WriteLine("ListRaceDetails {0} {1}", raceDetails.Response.First().MeetingId, raceDetails.Response.First().RaceStatus.ToString());

        var marketListener = MarketListener.Create(_client, BFHelpers.HorseRacePriceProjection(), 1);

        while (_markets.Count > 0)
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            MarketCatalogue marketCatalogue;
            _markets.TryDequeue(out marketCatalogue);

            var marketSubscription = marketListener.SubscribeMarketBook(marketCatalogue.MarketId)
                .SubscribeOn(Scheduler.Default)
                .Subscribe(
                tick =>
                {
                    Console.WriteLine(BFHelpers.MarketBookConsole(marketCatalogue, tick, marketCatalogue.Runners));
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
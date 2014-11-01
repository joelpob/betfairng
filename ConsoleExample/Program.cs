using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;
using BetfairNG;
using BetfairNG.Data;

// This example pulls the latest horse races in the UK markets
// and displays them to the console.
public class ConsoleExample
{
    public static ConcurrentQueue<MarketCatalogue> Markets = new ConcurrentQueue<MarketCatalogue>();

    public static void Main()
    {
        // TODO:// replace with your app details and Betfair username/password
        BetfairClient client = new BetfairClient(Exchange.UK, "APPKEY");
        client.Login(@"client-2048.p12", "certpass", "username", "password");

        // find all the upcoming UK horse races (EventTypeId 7)
        var marketFilter = new MarketFilter();
        marketFilter.EventTypeIds = new HashSet<string>() { "7" };
        marketFilter.MarketStartTime = new TimeRange()
        {
            From = DateTime.Now,
            To = DateTime.Now.AddDays(2)
        };
        marketFilter.MarketTypeCodes = new HashSet<String>() { "WIN" };

        Console.WriteLine("BetfairClient.ListTimeRanges()");
        var timeRanges = client.ListTimeRanges(marketFilter, TimeGranularity.HOURS).Result;
        if (timeRanges.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.ListCurrentOrders()");
        var currentOrders = client.ListCurrentOrders().Result;
        if (currentOrders.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.ListVenues()");
        var venues = client.ListVenues(marketFilter).Result;
        if (venues.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.GetAccountDetails()");
        var accountDetails = client.GetAccountDetails().Result;
        if (accountDetails.HasError)
            throw new ApplicationException();

        Console.WriteLine("BetfairClient.GetAccountStatement()");
        var accountStatement = client.GetAccountStatement().Result;
        if (accountStatement.HasError)
            throw new ApplicationException();

        Console.Write("BetfairClient.GetAccountFunds() ");
        var acc = client.GetAccountFunds(Wallet.UK).Result;
        if (acc.HasError)
            throw new ApplicationException();
        Console.WriteLine(acc.Response.AvailableToBetBalance);

        Console.WriteLine("BetfairClient.ListClearedOrders()");
        var clearedOrders = client.ListClearedOrders(BetStatus.SETTLED).Result;
        if (clearedOrders.HasError)
            throw new ApplicationException();

          var marketCatalogues = client.ListMarketCatalogue(
            BFHelpers.HorseRaceFilter(),
            BFHelpers.HorseRaceProjection(),
            MarketSort.FIRST_TO_START,
            25).Result.Response;

        marketCatalogues.ForEach(c =>
        {
            Markets.Enqueue(c);
            Console.WriteLine(c.MarketName);
        });
        Console.WriteLine();

        var marketListener = MarketListener.Create(client, BFHelpers.HorseRacePriceProjection(), 1);

        while (Markets.Count > 0)
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            MarketCatalogue marketCatalogue;
            Markets.TryDequeue(out marketCatalogue);

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

        Console.WriteLine("done.");
        Console.ReadLine();
    }
}

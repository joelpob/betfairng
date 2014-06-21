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
using BetfairNG;
using BetfairNG.Data;

public class TestAPI
{
    public static void Test()
    {
        BetfairClient client = new BetfairClient(Exchange.AUS, "QWERASDF1234");
        client.Login("client-2048.p12", "certpassword", "username", "password");

        var currencyRates = client.ListCurrencyRates("GBP").Result;

        var marketFilter = new MarketFilter();
        marketFilter.EventTypeIds = new HashSet<string>() { "7" };
        marketFilter.MarketStartTime = new TimeRange()
        {
            From = DateTime.Now,
            To = DateTime.Now.AddDays(2)
        };
        marketFilter.MarketTypeCodes = new HashSet<String>() { "WIN" };

        var timeRanges = client.ListTimeRanges(marketFilter, TimeGranularity.HOURS).Result;
        if (timeRanges.HasError)
            throw new ApplicationException();

        var currentOrders = client.ListCurrentOrders().Result;
        if (currentOrders.HasError)
            throw new ApplicationException();

        var venues = client.ListVenues(marketFilter).Result;
        if (venues.HasError)
            throw new ApplicationException();

        var accountDetails = client.GetAccountDetails().Result;
        if (accountDetails.HasError)
            throw new ApplicationException();

        var accountStatement = client.GetAccountStatement().Result;
        if (accountStatement.HasError)
            throw new ApplicationException();

        var acc = client.GetAccountFunds().Result;
        if (acc.HasError)
            throw new ApplicationException();

        var clearedOrders = client.ListClearedOrders(BetStatus.SETTLED).Result;
        if (clearedOrders.HasError)
            throw new ApplicationException();

        ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
        marketProjections.Add(MarketProjection.RUNNER_METADATA);
        marketProjections.Add(MarketProjection.EVENT);

        var marketCatalogues = client.ListMarketCatalogue(
            marketFilter,
            marketProjections,
            MarketSort.FIRST_TO_START,
            20).Result.Response;

        // we have a bunch of markets now
        marketCatalogues.ForEach(c =>
            Console.WriteLine(string.Format("{0} {1} {2}",
                c.Event.CountryCode,
                c.Event.Name,
                c.Event.Venue)));

        var marketIdsFirst = marketCatalogues.Select(c => c.MarketId).Take(1);

        ISet<PriceData> priceData = new HashSet<PriceData>();
        //get all prices from the exchange
        priceData.Add(PriceData.EX_TRADED);
        priceData.Add(PriceData.EX_ALL_OFFERS);

        var priceProjection = new PriceProjection();
        priceProjection.PriceData = priceData;

        var sub = MarketListener.Create(client, priceProjection, 2);
        var runner = marketCatalogues.First().Runners.First();

        var runnerTicks = sub.SubscribeRunner(marketCatalogues.First().MarketId, runner.SelectionId);
        runnerTicks.Take(5)
            .Subscribe(c => Console.WriteLine("{0} {1}", runner.RunnerName, c.LastPriceTraded));

        Console.WriteLine("done");
        Console.ReadLine();
    }
}

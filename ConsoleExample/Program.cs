using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Configuration;
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

        /*
         * OriginalExample runs the code originally in here, using the standard MarketListener
         * PeriodicExample runs a version of MarketListener (MarketListenerPeriodic), using an RX interval, specified in seconds
         * MultiPeriodExample runs a version of MarketListenerPeriodic (MarketListenerMultiPeriod), using potentially differing poll intervals per market book
         */

        //var example = new OriginalExample(client); // This example blocks within GO
        //var example = new PeriodicExample(client, 0.5);
        var example = new MultiPeriodExample(client);
        example.Go();

        if(!example.IsBlocking) Thread.Sleep(TimeSpan.FromMinutes(20));

        Console.WriteLine("done.");
        Console.ReadLine();
    }
}

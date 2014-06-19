# Betfair API-NG C# Client API

A feature complete .NET Betfair [API-NG] C# client that builds on the Betfair [sample code]. It uses the [Task Parallel Library] and [Reactive Extensions] for the concurrency layer.

How to use it
-------------

To login to Betfair using this library, you'll need both a) a self signed certificate (follow the process [described here]), and b) an application key [directions here]. 

```
BetfairClient client = new BetfairClient(Exchange.AUS, "ASDF1234qwerty");
client.Login("client-2048.p12", "certpass", "username", "password");
```

Check out the "TestAPI.cs" file for examples on how to use the "BetfairClient" class.

The "MarketListener" class is where the magic happens:

```

AutoResetEvent waitHandle = new AutoResetEvent(false);
var marketListener = MarketListener.Create(client, BFHelpers.HorseRacePriceProjection(), 1);

var marketCatalogues = client.ListMarketCatalogue(
    BFHelpers.HorseRaceFilter(), 
    BFHelpers.HorseRaceProjection(), 
    MarketSort.FIRST_TO_START,
    25).Result.Response;
    
var marketCatalogue = marketCatalogues.First();

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
```

MarketListener will poll the Betfair API-NG service (ListMarketBook) for all subscribed markets and push the result to all Rx subscribers. You can also subscribe to individual runners in a given market (MarketListener.SubscribeRunner()).

Enjoy.

[sample code]:https://github.com/betfair/API-NG-sample-code/tree/master/cSharp
[API-NG]:https://api.developer.betfair.com/services/webapps/docs/display/1smk3cen4v3lu3yomq5qye0ni/Getting+Started+with+API-NG
[Reactive Extensions]:https://github.com/Reactive-Extensions
[Task Parallel Library]:http://msdn.microsoft.com/en-us/library/dd460717(v=vs.110).aspx
[described here]:https://api.developer.betfair.com/services/webapps/docs/display/1smk3cen4v3lu3yomq5qye0ni/Non-Interactive+(bot)+login
[directions here]:https://api.developer.betfair.com/services/webapps/docs/display/1smk3cen4v3lu3yomq5qye0ni/Application+Keys
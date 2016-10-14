# Betfair API-NG C# Client API

A feature complete .NET Betfair [API-NG] C# client that builds on the Betfair [sample code]. It uses the [Task Parallel Library] and [Reactive Extensions] for the concurrency layer. 
Now with [Exchange Streaming] API support (early beta). Be sure to pull in the Betfair example code submodule:

```sh
git clone https://github.com/joelpob/betfairng
cd betfairng
git submodule init
git submodule update
```

How to use it
-------------

To login to Betfair using this library, you'll need both a) a self signed certificate (follow the process [described here]), and b) an application key [directions here]. 

```c#
BetfairClient client = new BetfairClient("ASDF1234qwerty");
client.Login("client-2048.p12", "certpass", "username", "password");
```

Check out the ConsoleExample project (in the solution) for an example on how to use the "BetfairClient" class. If you're unfamiliar with the Task Parallel Library or Reactive Extensions, you can use the "BetfairClientSync" class. 
 
The "MarketListener" class is where the magic happens:

```c#
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

waitHandle.WaitOne();
marketSubscription.Dispose();
```

MarketListener will poll the Betfair API-NG service (ListMarketBook) for all subscribed markets and push the result to all Rx subscribers. You can also subscribe to individual runners in a given market (MarketListener.SubscribeRunner()).

Enjoy.

[sample code]:https://github.com/betfair/API-NG-sample-code/tree/master/cSharp
[API-NG]:https://api.developer.betfair.com/services/webapps/docs/display/1smk3cen4v3lu3yomq5qye0ni/Getting+Started+with+API-NG
[Exchange Streaming]:http://docs.developer.betfair.com/docs/display/1smk3cen4v3lu3yomq5qye0ni/Exchange+Stream+API
[Reactive Extensions]:https://github.com/Reactive-Extensions
[Task Parallel Library]:http://msdn.microsoft.com/en-us/library/dd460717(v=vs.110).aspx
[described here]:https://api.developer.betfair.com/services/webapps/docs/display/1smk3cen4v3lu3yomq5qye0ni/Non-Interactive+(bot)+login
[directions here]:https://api.developer.betfair.com/services/webapps/docs/display/1smk3cen4v3lu3yomq5qye0ni/Application+Keys

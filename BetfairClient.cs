using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetfairNG.Data;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace BetfairNG
{
    public class BetfairClient
    {
        private Exchange exchange;
        private Network networkClient;
        private string appKey;
        private string sessionToken;
        private Action preNetworkRequest;
        private WebProxy proxy;
        private static TraceSource trace = new TraceSource("BetfairClient");

        private static readonly string LIST_COMPETITIONS_METHOD = "SportsAPING/v1.0/listCompetitions";
        private static readonly string LIST_COUNTRIES_METHOD = "SportsAPING/v1.0/listCountries";
        private static readonly string LIST_CURRENT_ORDERS_METHOD = "SportsAPING/v1.0/listCurrentOrders";
        private static readonly string LIST_CLEARED_ORDERS_METHOD = "SportsAPING/v1.0/listClearedOrders";
        private static readonly string LIST_EVENT_TYPES_METHOD = "SportsAPING/v1.0/listEventTypes";
        private static readonly string LIST_MARKET_CATALOGUE_METHOD = "SportsAPING/v1.0/listMarketCatalogue";
        private static readonly string LIST_MARKET_BOOK_METHOD = "SportsAPING/v1.0/listMarketBook";
        private static readonly string LIST_MARKET_PROFIT_AND_LOSS = "SportsAPING/v1.0/listMarketProfitAndLoss";
        private static readonly string LIST_MARKET_TYPES = "SportsAPING/v1.0/listMarketTypes";
        private static readonly string LIST_TIME_RANGES = "SportsAPING/v1.0/listTimeRanges";
        private static readonly string LIST_VENUES = "SportsAPING/v1.0/listVenues";
        private static readonly string PLACE_ORDERS_METHOD = "SportsAPING/v1.0/placeOrders";
        private static readonly string CANCEL_ORDERS_METHOD = "SportsAPING/v1.0/cancelOrders";
        private static readonly string REPLACE_ORDERS_METHOD = "SportsAPING/v1.0/replaceOrders";
        private static readonly string UPDATE_ORDERS_METHOD = "SportsAPING/v1.0/updateOrders";

        private static readonly string GET_ACCOUNT_DETAILS = "AccountAPING/v1.0/getAccountDetails";
        private static readonly string GET_ACCOUNT_FUNDS = "AccountAPING/v1.0/getAccountFunds";
        private static readonly string GET_ACCOUNT_STATEMENT = "AccountAPING/v1.0/getAccountStatement";
        private static readonly string LIST_CURRENCY_RATES = "AccountAPING/v1.0/listCurrencyRates";
        private static readonly string TRANSFER_FUNDS = "AccountAPING/v1.0/transferFunds";

        private static readonly string FILTER = "filter";
        private static readonly string BET_IDS = "betIds";
        private static readonly string RUNNER_IDS = "runnerIds";
        private static readonly string SIDE = "side";
        private static readonly string SETTLED_DATE_RANGE = "settledDateRange";
        private static readonly string EVENT_TYPE_IDS = "eventTypeIds";
        private static readonly string EVENT_IDS = "eventIds";
        private static readonly string BET_STATUS = "betStatus";
        private static readonly string PLACED_DATE_RANGE = "placedDateRange";
        private static readonly string DATE_RANGE = "dateRange";
        private static readonly string ORDER_BY = "orderBy";
        private static readonly string GROUP_BY = "groupBy";
        private static readonly string SORT_DIR = "sortDir";
        private static readonly string FROM_RECORD = "fromRecord";
        private static readonly string RECORD_COUNT = "recordCount";
        private static readonly string GRANULARITY = "granularity";
        private static readonly string MARKET_PROJECTION = "marketProjection";
        private static readonly string MATCH_PROJECTION = "matchProjection";
        private static readonly string ORDER_PROJECTION = "orderProjection";
        private static readonly string PRICE_PROJECTION = "priceProjection";
        private static readonly string SORT = "sort";
        private static readonly string MAX_RESULTS = "maxResults";
        private static readonly string MARKET_IDS = "marketIds";
        private static readonly string MARKET_ID = "marketId";
        private static readonly string INSTRUCTIONS = "instructions";
        private static readonly string CUSTOMER_REFERENCE = "customerRef";
        private static readonly string INCLUDE_SETTLED_BETS = "includeSettledBets";
        private static readonly string INCLUDE_BSP_BETS = "includeBspBets";
        private static readonly string INCLUDE_ITEM_DESCRIPTION = "includeItemDescription";
        private static readonly string NET_OF_COMMISSION = "netOfCommission";
        private static readonly string FROM_CURRENCY = "fromCurrency";
        private static readonly string FROM = "from";
        private static readonly string TO = "to";
        private static readonly string AMOUNT = "amount";
        private static readonly string WALLET = "wallet";

        public BetfairClient(Exchange exchange, string appKey, Action preNetworkRequest = null, WebProxy proxy = null)
        {
            if (string.IsNullOrWhiteSpace(appKey)) throw new ArgumentException("appKey");

            this.exchange = exchange;
            this.appKey = appKey;
            this.preNetworkRequest = preNetworkRequest;
            this.proxy = proxy;
        }

        public BetfairClient(
            Exchange exchange, 
            string appKey, 
            string sessionToken, 
            Action preNetworkRequest = null,
            WebProxy proxy = null)
        {
            if (string.IsNullOrWhiteSpace(appKey)) throw new ArgumentException("appKey");
            if (string.IsNullOrWhiteSpace(sessionToken)) throw new ArgumentException("sessionToken");

            this.exchange = exchange;
            this.appKey = appKey;
            this.sessionToken = sessionToken;
            this.preNetworkRequest = preNetworkRequest;
            this.proxy = proxy;
            this.networkClient = new Network(this.appKey, this.sessionToken, this.preNetworkRequest, true, this.proxy);
        }
        
        public bool Login(string p12CertificateLocation, string p12CertificatePassword, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(p12CertificateLocation)) throw new ArgumentException("p12CertificateLocation");
            if (string.IsNullOrWhiteSpace(p12CertificatePassword)) throw new ArgumentException("p12CertificatePassword");
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("password");
            if (!File.Exists(p12CertificateLocation)) throw new ArgumentException("p12CertificateLocation not found");

            if (preNetworkRequest != null)
                preNetworkRequest();

            string postData = string.Format("username={0}&password={1}", username, password);
            X509Certificate2 x509certificate = new X509Certificate2(p12CertificateLocation, p12CertificatePassword);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://identitysso-api.betfair.com/api/certlogin");
            request.UseDefaultCredentials = true;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("X-Application", appKey);
            request.ClientCertificates.Add(x509certificate);
            request.Accept = "*/*";
            if (this.proxy != null)
                request.Proxy = this.proxy;

            using (Stream stream = request.GetRequestStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Default))
            {
                writer.Write(postData);
            }

            using (Stream stream = ((HttpWebResponse)request.GetResponse()).GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.Default))
            {
                var jsonResponse = JsonConvert.Deserialize<LoginResponse>(reader.ReadToEnd());
                if (jsonResponse.LoginStatus == "SUCCESS")
                {
                    this.sessionToken = jsonResponse.SessionToken;
                    this.networkClient = new Network(appKey, this.sessionToken, this.preNetworkRequest);

                    return true;
                }
                else
                    return false;
            }
        }

        public Task<BetfairServerResponse<List<CompetitionResult>>> ListCompetitions(MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            return networkClient.Invoke<List<CompetitionResult>>(exchange, Endpoint.Betting, LIST_COUNTRIES_METHOD, args);
        }

        public Task<BetfairServerResponse<List<CountryCodeResult>>> ListCountries(MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            return networkClient.Invoke<List<CountryCodeResult>>(exchange, Endpoint.Betting, LIST_COMPETITIONS_METHOD, args);
        }

        public Task<BetfairServerResponse<CurrentOrderSummaryReport>> ListCurrentOrders(
            ISet<string> betIds = null,
            ISet<string> marketIds = null,
            OrderProjection? orderProjection = null,
            TimeRange placedDateRange = null,
            TimeRange dateRange = null,
            OrderBy? orderBy = null,
            SortDir? sortDir = null,
            int? fromRecord = null,
            int? recordCount = null)
        {
            var args = new Dictionary<string, object>();
            args[BET_IDS] = betIds;
            args[MARKET_IDS] = marketIds;
            args[ORDER_PROJECTION] = orderProjection;
            args[PLACED_DATE_RANGE] = placedDateRange;
            args[DATE_RANGE] = dateRange;
            args[ORDER_BY] = orderBy;
            args[SORT_DIR] = sortDir;
            args[FROM_RECORD] = fromRecord;
            args[RECORD_COUNT] = recordCount;
            return networkClient.Invoke<CurrentOrderSummaryReport>(exchange, Endpoint.Betting, LIST_CURRENT_ORDERS_METHOD, args);
        }

        public Task<BetfairServerResponse<ClearedOrderSummaryReport>> ListClearedOrders(
            BetStatus betStatus,
            ISet<string> eventTypeIds = null,
            ISet<string> eventIds = null,
            ISet<string> marketIds = null,
            ISet<RunnerId> runnerIds = null,
            ISet<string> betIds = null,
            Side? side = null,
            TimeRange settledDateRange = null,
            GroupBy? groupBy = null,
            bool? includeItemDescription = null,
            int? fromRecord = null,
            int? recordCount = null)
        {
            var args = new Dictionary<string, object>();
            args[BET_STATUS] = betStatus;
            args[EVENT_TYPE_IDS] = eventTypeIds;
            args[EVENT_IDS] = eventIds;
            args[MARKET_IDS] = marketIds;
            args[RUNNER_IDS] = runnerIds;
            args[BET_IDS] = betIds;
            args[SIDE] = side;
            args[SETTLED_DATE_RANGE] = settledDateRange;
            args[GROUP_BY] = groupBy;
            args[INCLUDE_ITEM_DESCRIPTION] = includeItemDescription;
            args[FROM_RECORD] = fromRecord;
            args[RECORD_COUNT] = recordCount;

            return networkClient.Invoke<ClearedOrderSummaryReport>(exchange, Endpoint.Betting, LIST_CLEARED_ORDERS_METHOD, args);
        }

        public Task<BetfairServerResponse<List<EventResult>>> ListEvents(MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            return networkClient.Invoke<List<EventResult>>(exchange, Endpoint.Betting, LIST_EVENT_TYPES_METHOD, args);
        }

        public Task<BetfairServerResponse<List<EventTypeResult>>> ListEventTypes(MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            return networkClient.Invoke<List<EventTypeResult>>(exchange, Endpoint.Betting, LIST_EVENT_TYPES_METHOD, args);
        }

        public Task<BetfairServerResponse<List<MarketBook>>> ListMarketBook(
            IEnumerable<string> marketIds,
            PriceProjection priceProjection = null,
            OrderProjection? orderProjection = null,
            MatchProjection? matchProjection = null)
        {
            var args = new Dictionary<string, object>();
            args[MARKET_IDS] = marketIds;
            args[PRICE_PROJECTION] = priceProjection;
            args[ORDER_PROJECTION] = orderProjection;
            args[MATCH_PROJECTION] = matchProjection;
            return networkClient.Invoke<List<MarketBook>>(exchange, Endpoint.Betting, LIST_MARKET_BOOK_METHOD, args);
        }

        public Task<BetfairServerResponse<List<MarketCatalogue>>> ListMarketCatalogue(
            MarketFilter marketFilter, 
            ISet<MarketProjection> marketProjections = null, 
            MarketSort? sort = null, 
            int maxResult = 1)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            args[MARKET_PROJECTION] = marketProjections;
            args[SORT] = sort;
            args[MAX_RESULTS] = maxResult;
            return networkClient.Invoke<List<MarketCatalogue>>(exchange, Endpoint.Betting, LIST_MARKET_CATALOGUE_METHOD, args);
        }

        public Task<BetfairServerResponse<List<MarketProfitAndLoss>>> ListMarketProfitAndLoss(
            ISet<string> marketIds,
            bool includeSettledBets,
            bool includeBsbBets,
            bool netOfCommission)
        {
            var args = new Dictionary<string, object>();
            args[MARKET_IDS] = marketIds;
            args[INCLUDE_SETTLED_BETS] = includeSettledBets;
            args[INCLUDE_BSP_BETS] = includeBsbBets;
            args[NET_OF_COMMISSION] = netOfCommission;
            return networkClient.Invoke<List<MarketProfitAndLoss>>(exchange, Endpoint.Betting, LIST_MARKET_PROFIT_AND_LOSS, args);
        }

        public Task<BetfairServerResponse<List<MarketTypeResult>>> ListMarketTypes(MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            return networkClient.Invoke<List<MarketTypeResult>>(exchange, Endpoint.Betting, LIST_MARKET_TYPES, args);
        }

        public Task<BetfairServerResponse<List<TimeRangeResult>>> ListTimeRanges(MarketFilter marketFilter, TimeGranularity timeGranularity)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            args[GRANULARITY] = timeGranularity;
            return networkClient.Invoke<List<TimeRangeResult>>(exchange, Endpoint.Betting, LIST_TIME_RANGES, args);
        }

        public Task<BetfairServerResponse<List<VenueResult>>> ListVenues(MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[FILTER] = marketFilter;
            return networkClient.Invoke<List<VenueResult>>(exchange, Endpoint.Betting, LIST_VENUES, args);
        }

        public Task<BetfairServerResponse<PlaceExecutionReport>> PlaceOrders(
            string marketId, 
            IList<PlaceInstruction> placeInstructions,
            string customerRef = null)
        {
            var args = new Dictionary<string, object>();

            args[MARKET_ID] = marketId;
            args[INSTRUCTIONS] = placeInstructions;
            args[CUSTOMER_REFERENCE] = customerRef;

            return networkClient.Invoke<PlaceExecutionReport>(exchange, Endpoint.Betting, PLACE_ORDERS_METHOD, args);
        }

        public Task<BetfairServerResponse<CancelExecutionReport>> CancelOrders(
            string marketId = null,
            IList<CancelInstruction> instructions = null,
            string customerRef = null)
        {
            var args = new Dictionary<string, object>();

            args[INSTRUCTIONS] = instructions;
            args[MARKET_ID] = marketId;
            args[CUSTOMER_REFERENCE] = customerRef;

            return networkClient.Invoke<CancelExecutionReport>(exchange, Endpoint.Betting, CANCEL_ORDERS_METHOD, args);
        }

        public Task<BetfairServerResponse<ReplaceExecutionReport>> ReplaceOrders(
            string marketId,
            IList<ReplaceInstruction> instructions,
            string customerRef = null)
        {
            var args = new Dictionary<string, object>();

            args[MARKET_ID] = marketId;
            args[INSTRUCTIONS] = instructions;
            args[CUSTOMER_REFERENCE] = customerRef;

            return networkClient.Invoke<ReplaceExecutionReport>(exchange, Endpoint.Betting, REPLACE_ORDERS_METHOD, args);
        }

        public Task<BetfairServerResponse<UpdateExecutionReport>> UpdateOrders(
            string marketId,
            IList<UpdateInstruction> instructions,
            string customerRef = null)
        {
            var args = new Dictionary<string, object>();

            args[MARKET_ID] = marketId;
            args[INSTRUCTIONS] = instructions;
            args[CUSTOMER_REFERENCE] = customerRef;

            return networkClient.Invoke<UpdateExecutionReport>(exchange, Endpoint.Betting, UPDATE_ORDERS_METHOD, args);
        }

        public Task<BetfairServerResponse<AccountDetailsResponse>> GetAccountDetails()
        {
            var args = new Dictionary<string, object>();
            return networkClient.Invoke<AccountDetailsResponse>(exchange, Endpoint.Account, GET_ACCOUNT_DETAILS, args);
        }

        public Task<BetfairServerResponse<AccountFundsResponse>> GetAccountFunds(Wallet wallet)
        {
            var args = new Dictionary<string, object>();
            args[WALLET] = wallet;
            return networkClient.Invoke<AccountFundsResponse>(exchange, Endpoint.Account, GET_ACCOUNT_FUNDS, args);
        }

        public Task<BetfairServerResponse<AccountStatementReport>> GetAccountStatement(
            int? fromRecord = null,
            int? recordCount = null,
            TimeRange itemDateRange = null,
            IncludeItem? includeItem = null,
            Wallet? wallet = null)
        {
            var args = new Dictionary<string, object>();
            return networkClient.Invoke<AccountStatementReport>(exchange, Endpoint.Account, GET_ACCOUNT_STATEMENT, args);
        }

        public Task<BetfairServerResponse<List<CurrencyRate>>> ListCurrencyRates(string fromCurrency)
        {
            var args = new Dictionary<string, object>();
            args[FROM_CURRENCY] = fromCurrency;
            return networkClient.Invoke<List<CurrencyRate>>(exchange, Endpoint.Account, LIST_CURRENCY_RATES, args);
        }

        public Task<BetfairServerResponse<TransferResponse>> TransferFunds(Wallet from, Wallet to, double amount)
        {
            var args = new Dictionary<string, object>();
            args[FROM] = from;
            args[TO] = to;
            args[AMOUNT] = amount;
            return networkClient.Invoke<TransferResponse>(exchange, Endpoint.Account, TRANSFER_FUNDS, args);
        }
    }

    public enum Exchange
    {
        UK,
        AUS
    }

    public enum Endpoint
    {
        Betting,
        Account
    }

    public class LoginResponse
    {
        [JsonProperty(PropertyName = "sessionToken")]
        public string SessionToken { get; set; }

        [JsonProperty(PropertyName = "loginStatus")]
        public string LoginStatus { get; set; }
    }

    public class BetfairServerResponse<T>
    {
        public T Response { get; set; }
        public DateTime LastByte { get; set; }
        public DateTime RequestStart { get; set; }
        public long LatencyMS { get; set; }
        public bool HasError { get; set; }
        public BetfairServerException Error { get; set; }
    }

    public class BetfairServerException : System.Exception
    {
        public JObject ServerData { get; set; }
        public JObject ServerDetail { get; set; }

        public static BetfairServerException ToClientException(Data.Exceptions.Exception ex)
        {
            if (ex == null)
                return null;

            var exception = new BetfairServerException();
            exception.ServerData = ex.Data;
            exception.ServerDetail = ex.Detail;
            return exception;
        }
    }
}

using BetfairNG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BetfairNG
{
    /// <summary>
    /// Synchronous version of the Betfair client. 
    /// </summary>
    public class BetfairClientSync
    {
        private BetfairClient client;

        public BetfairClientSync(Exchange exchange, 
            string appKey, 
            string sessionToken, 
            Action preNetworkRequest = null,
            WebProxy proxy = null)
        { 
            client = new BetfairClient(exchange, appKey, sessionToken, preNetworkRequest, proxy);
        }

        public BetfairClientSync(Exchange exchange,
            string appKey,
            Action preNetworkRequest = null,
            WebProxy proxy = null)
        {
            client = new BetfairClient(exchange, appKey, preNetworkRequest, proxy);
        }

        public bool Login(string p12CertificateLocation, string p12CertificatePassword, string username, string password)
        {
            return client.Login(p12CertificateLocation, p12CertificatePassword, username, password);
        }

        public BetfairServerResponse<List<CompetitionResult>> ListCompetitions(MarketFilter marketFilter)
        {
            return client.ListCompetitions(marketFilter).Result;
        }

        public BetfairServerResponse<List<CountryCodeResult>> ListCountries(MarketFilter marketFilter)
        {
            return client.ListCountries(marketFilter).Result;
        }

        public BetfairServerResponse<CurrentOrderSummaryReport> ListCurrentOrders(
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
            return client.ListCurrentOrders(
                betIds,
                marketIds,
                orderProjection,
                placedDateRange,
                dateRange,
                orderBy,
                sortDir,
                fromRecord,
                recordCount).Result;
        }

        public BetfairServerResponse<ClearedOrderSummaryReport> ListClearedOrders(
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
            return client.ListClearedOrders(
                betStatus,
                eventTypeIds,
                eventIds,
                marketIds,
                runnerIds,
                betIds,
                side,
                settledDateRange,
                groupBy,
                includeItemDescription,
                fromRecord,
                recordCount).Result;
        }

        public BetfairServerResponse<List<EventResult>> ListEvents(MarketFilter marketFilter)
        {
            return client.ListEvents(marketFilter).Result;
        }

        public BetfairServerResponse<List<EventTypeResult>> ListEventTypes(MarketFilter marketFilter)
        {
            return client.ListEventTypes(marketFilter).Result;
        }

        public BetfairServerResponse<List<MarketBook>> ListMarketBook(
            IEnumerable<string> marketIds,
            PriceProjection priceProjection = null,
            OrderProjection? orderProjection = null,
            MatchProjection? matchProjection = null)
        {
            return client.ListMarketBook(
                marketIds,
                priceProjection,
                orderProjection,
                matchProjection).Result;
        }

        public BetfairServerResponse<List<MarketCatalogue>> ListMarketCatalogue(
            MarketFilter marketFilter,
            ISet<MarketProjection> marketProjections = null,
            MarketSort? sort = null,
            int maxResult = 1)
        {
            return client.ListMarketCatalogue(
                marketFilter,
                marketProjections,
                sort,
                maxResult).Result;
        }

        public BetfairServerResponse<List<MarketProfitAndLoss>> ListMarketProfitAndLoss(
            ISet<string> marketIds,
            bool includeSettledBets,
            bool includeBsbBets,
            bool netOfCommission)
        {
            return client.ListMarketProfitAndLoss(
                marketIds,
                includeSettledBets,
                includeBsbBets,
                netOfCommission).Result;
        }

        public BetfairServerResponse<List<MarketTypeResult>> ListMarketTypes(MarketFilter marketFilter)
        {
            return client.ListMarketTypes(marketFilter).Result;
        }

        public BetfairServerResponse<List<TimeRangeResult>> ListTimeRanges(MarketFilter marketFilter, TimeGranularity timeGranularity)
        {
            return client.ListTimeRanges(marketFilter, timeGranularity).Result;
        }

        public BetfairServerResponse<List<VenueResult>> ListVenues(MarketFilter marketFilter)
        {
            return client.ListVenues(marketFilter).Result;
        }

         public BetfairServerResponse<PlaceExecutionReport> PlaceOrders(
            string marketId, 
            IList<PlaceInstruction> placeInstructions,
            string customerRef = null)
        {
            return client.PlaceOrders(marketId, placeInstructions, customerRef).Result;
        }

        public BetfairServerResponse<CancelExecutionReport> CancelOrders(
            string marketId = null,
            IList<CancelInstruction> instructions = null,
            string customerRef = null)
         {
             return client.CancelOrders(marketId, instructions, customerRef).Result;
         }

        public BetfairServerResponse<ReplaceExecutionReport> ReplaceOrders(
            string marketId,
            IList<ReplaceInstruction> instructions,
            string customerRef = null)
        {
            return client.ReplaceOrders(marketId, instructions, customerRef).Result;
        }

        public BetfairServerResponse<UpdateExecutionReport> UpdateOrders(
           string marketId,
           IList<UpdateInstruction> instructions,
           string customerRef = null)
        {
            return client.UpdateOrders(marketId, instructions, customerRef).Result;
        }

        public BetfairServerResponse<AccountDetailsResponse> GetAccountDetails()
        {
            return client.GetAccountDetails().Result;
        }

        public BetfairServerResponse<AccountFundsResponse> GetAccountFunds(Wallet wallet)
        {
            return client.GetAccountFunds(wallet).Result;
        }

        public BetfairServerResponse<AccountStatementReport> GetAccountStatement(
            int? fromRecord = null,
            int? recordCount = null,
            TimeRange itemDateRange = null,
            IncludeItem? includeItem = null,
            Wallet? wallet = null)
        {
            return client.GetAccountStatement(
                fromRecord,
                recordCount,
                itemDateRange,
                includeItem,
                wallet).Result;
        }

        public BetfairServerResponse<List<CurrencyRate>> ListCurrencyRates(string fromCurrency)
        {
            return client.ListCurrencyRates(fromCurrency).Result;
        }

        public BetfairServerResponse<TransferResponse> TransferFunds(Wallet from, Wallet to, double amount)
        {
            return client.TransferFunds(from, to, amount).Result;
        }
    }
}

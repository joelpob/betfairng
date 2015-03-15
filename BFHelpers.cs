using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;
using BetfairNG.Data;

namespace BetfairNG
{
    public static class BFHelpers
    {
        public static MarketFilter HorseRaceFilter(string country = null)
        {
            var marketFilter = new MarketFilter();
            marketFilter.EventTypeIds = new HashSet<string>() { "7" };
            marketFilter.MarketStartTime = new TimeRange()
            {
                From = DateTime.Now,
                To = DateTime.Now.AddDays(1)
            };
            if (country != null)
                marketFilter.MarketCountries = new HashSet<string>() { country };
            marketFilter.MarketTypeCodes = new HashSet<String>() { "WIN" };

            return marketFilter;
        }

        public static PriceProjection HorseRacePriceProjection()
        {
            ISet<PriceData> priceData = new HashSet<PriceData>();
            //get all prices from the exchange
            priceData.Add(PriceData.EX_TRADED);
            priceData.Add(PriceData.EX_ALL_OFFERS);

            var priceProjection = new PriceProjection();
            priceProjection.PriceData = priceData;
            return priceProjection;
        }

        public static ISet<MarketProjection> HorseRaceProjection()
        {
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.RUNNER_METADATA);
            marketProjections.Add(MarketProjection.MARKET_DESCRIPTION);
            marketProjections.Add(MarketProjection.EVENT);

            return marketProjections;
        }

        public static double GetMarketEfficiency(IEnumerable<double> odds)
        {
            double total = odds.Sum(c => 1.0 / c);
            return 1.0 / total;
        }

        public static double Best(this List<PriceSize> prices)
        {
            if (prices.Count > 0)
                return prices.First().Price;
            else
                return 0.0;
        }

        public static List<Order> Backs(this List<Order> orders)
        {
            return orders.Where(c => c.Side == Side.BACK).ToList();
        }

        public static List<Order> Lays(this List<Order> orders)
        {
            return orders.Where(c => c.Side == Side.LAY).ToList();
        }

        public static List<T> Copy<T>(this List<T> list)
        {
            List<T> newList = new List<T>();
            for (int i = 0; i < list.Count; i++)
                newList.Add(list[i]);

            return newList;
        }

        public static string MarketBookConsole(
            MarketCatalogue marketCatalogue,
            MarketBook marketBook,
            IEnumerable<RunnerDescription> runnerDescriptions,
            Func<RunnerDescription, Runner, string> backSide = null,
            Func<RunnerDescription, Runner, string> laySide = null)
        {
            var nearestBacks = marketBook.Runners
                .Where(c => c.Status == RunnerStatus.ACTIVE)
                .Select(c => c.ExchangePrices.AvailableToBack.Count > 0 ? c.ExchangePrices.AvailableToBack.First().Price : 0.0);
            var nearestLays = marketBook.Runners
                .Where(c => c.Status == RunnerStatus.ACTIVE)
                .Select(c => c.ExchangePrices.AvailableToLay.Count > 0 ? c.ExchangePrices.AvailableToLay.First().Price : 0.0);

            var timeToJump = Convert.ToDateTime(marketCatalogue.Event.OpenDate);
            var timeRemainingToJump = timeToJump.Subtract(DateTime.UtcNow);

            var sb = new StringBuilder()
                        .AppendFormat("{0} {1}", marketCatalogue.Event.Name, marketCatalogue.MarketName)
                        .AppendFormat(" : {0}% {1}%", BFHelpers.GetMarketEfficiency(nearestBacks).ToString("0.##"), BFHelpers.GetMarketEfficiency(nearestLays).ToString("0.##"))
                        .AppendFormat(" : Status={0}", marketBook.Status)
                        .AppendFormat(" : IsInplay={0}", marketBook.IsInplay)
                        .AppendFormat(" : Runners={0}", marketBook.NumberOfActiveRunners)
                        .AppendFormat(" : Matched={0}", marketBook.TotalMatched.ToString("C0"))
                        .AppendFormat(" : Avail={0}", marketBook.TotalAvailable.ToString("C0"));
            sb.AppendLine();
            sb.AppendFormat("Time To Jump: {0}h {1}:{2}",
                  timeRemainingToJump.Hours + (timeRemainingToJump.Days * 24),
                  timeRemainingToJump.Minutes.ToString("##"),
                  timeRemainingToJump.Seconds.ToString("##"));
            sb.AppendLine();

            if (marketBook.Runners != null && marketBook.Runners.Count > 0)
            {
                foreach (var runner in marketBook.Runners.Where(c => c.Status == RunnerStatus.ACTIVE))
                {
                    var runnerName = runnerDescriptions != null ? runnerDescriptions.FirstOrDefault(c => c.SelectionId == runner.SelectionId) : null;
                    var bsString = backSide != null ? backSide(runnerName, runner) : "";
                    var lyString = laySide != null ? laySide(runnerName, runner) : "";

                    string consoleRunnerName = runnerName != null ? runnerName.RunnerName : "null";

                    sb.AppendLine(string.Format("{0} {9} [{1}] {2},{3},{4}  ::  {5},{6},{7} [{8}] {10}",
                        consoleRunnerName.PadRight(25),
                        runner.ExchangePrices.AvailableToBack.Sum(a => a.Size).ToString("0").PadLeft(7),
                        runner.ExchangePrices.AvailableToBack.Count > 2 ? runner.ExchangePrices.AvailableToBack[2].Price.ToString("0.00").PadLeft(6) : "  0.00",
                        runner.ExchangePrices.AvailableToBack.Count > 1 ? runner.ExchangePrices.AvailableToBack[1].Price.ToString("0.00").PadLeft(6) : "  0.00",
                        runner.ExchangePrices.AvailableToBack.Count > 0 ? runner.ExchangePrices.AvailableToBack[0].Price.ToString("0.00").PadLeft(6) : "  0.00",
                        runner.ExchangePrices.AvailableToLay.Count > 0 ? runner.ExchangePrices.AvailableToLay[0].Price.ToString("0.00").PadLeft(6) : "  0.00",
                        runner.ExchangePrices.AvailableToLay.Count > 1 ? runner.ExchangePrices.AvailableToLay[1].Price.ToString("0.00").PadLeft(6) : "  0.00",
                        runner.ExchangePrices.AvailableToLay.Count > 2 ? runner.ExchangePrices.AvailableToLay[2].Price.ToString("0.00").PadLeft(6) : "  0.00",
                        runner.ExchangePrices.AvailableToLay.Sum(a => a.Size).ToString("0").PadLeft(7),
                        bsString,
                        lyString));
                }
            }

            return sb.ToString();
        }

        public static string ToStringRunnerName(IEnumerable<RunnerDescription> descriptions, IEnumerable<Runner> runners)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var runner in runners)
            {
                var nameRunner = descriptions.First(c => c.SelectionId == runner.SelectionId);

                builder.AppendLine(string.Format("{0}\t [{1}] {2},{3},{4}  ::  {5},{6},{7} [{8}]",
                    nameRunner.RunnerName.PadRight(25),
                    runner.ExchangePrices.AvailableToBack.Sum(a => a.Size).ToString().PadLeft(7),
                    runner.ExchangePrices.AvailableToBack.Count > 2 ? runner.ExchangePrices.AvailableToBack[2].Price.ToString("0.00").PadLeft(6) : "  0.00",
                    runner.ExchangePrices.AvailableToBack.Count > 1 ? runner.ExchangePrices.AvailableToBack[1].Price.ToString("0.00").PadLeft(6) : "  0.00",
                    runner.ExchangePrices.AvailableToBack.Count > 0 ? runner.ExchangePrices.AvailableToBack[0].Price.ToString("0.00").PadLeft(6) : "  0.00",
                    runner.ExchangePrices.AvailableToLay.Count > 0 ? runner.ExchangePrices.AvailableToLay[0].Price.ToString("0.00").PadLeft(6) : "  0.00",
                    runner.ExchangePrices.AvailableToLay.Count > 1 ? runner.ExchangePrices.AvailableToLay[1].Price.ToString("0.00").PadLeft(6) : "  0.00",
                    runner.ExchangePrices.AvailableToLay.Count > 2 ? runner.ExchangePrices.AvailableToLay[2].Price.ToString("0.00").PadLeft(6) : "  0.00",
                    runner.ExchangePrices.AvailableToLay.Sum(a => a.Size).ToString().PadLeft(7)));
            }

            return builder.ToString();
        }
    }

    public class PriceHelpers
    {
        public static double[] Table = new double[] 
        {
            1.01, 1.02, 1.03, 1.04, 1.05, 1.06, 1.07, 1.08, 1.09,
            1.1, 1.11, 1.12, 1.13, 1.14, 1.15, 1.16, 1.17, 1.18, 1.19, 1.2,
            1.21, 1.22, 1.23, 1.24, 1.25, 1.26, 1.27, 1.28, 1.29, 1.3, 1.31,
            1.32, 1.33, 1.34, 1.35, 1.36, 1.37, 1.38, 1.39, 1.4, 1.41, 1.42,
            1.43, 1.44, 1.45, 1.46, 1.47, 1.48, 1.49, 1.5, 1.51, 1.52, 1.53,
            1.54, 1.55, 1.56, 1.57, 1.58, 1.59, 1.6, 1.61, 1.62, 1.63, 1.64,
            1.65, 1.66, 1.67, 1.68, 1.69, 1.7, 1.71, 1.72, 1.73, 1.74, 1.75,
            1.76, 1.77, 1.78, 1.79, 1.8, 1.81, 1.82, 1.83, 1.84, 1.85, 1.86,
            1.87, 1.88, 1.89, 1.9, 1.91, 1.92, 1.93, 1.94, 1.95, 1.96, 1.97,
            1.98, 1.99, 2.0, 2.02, 2.04, 2.06, 2.08, 2.1, 2.12, 2.14, 2.16,
            2.18, 2.2, 2.22, 2.24, 2.26, 2.28, 2.3, 2.32, 2.34, 2.36, 2.38, 2.4,
            2.42, 2.44, 2.46, 2.48, 2.5, 2.52, 2.54, 2.56, 2.58, 2.6, 2.62,
            2.64, 2.66, 2.68, 2.7, 2.72, 2.74, 2.76, 2.78, 2.8, 2.82, 2.84,
            2.86, 2.88, 2.9, 2.92, 2.94, 2.96, 2.98, 3.0, 3.05, 3.1, 3.15, 3.2,
            3.25, 3.3, 3.35, 3.4, 3.45, 3.5, 3.55, 3.6, 3.65, 3.7, 3.75, 3.8,
            3.85, 3.9, 3.95, 4.0, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8, 4.9,
            5.0, 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8, 5.9, 6.0, 6.2, 6.4,
            6.6, 6.8, 7.0, 7.2, 7.4, 7.6, 7.8, 8.0, 8.2, 8.4, 8.6, 8.8, 9.0,
            9.2, 9.4, 9.6, 9.8, 10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5,
            14.0, 14.5, 15.0, 15.5, 16.0, 16.5, 17.0, 17.5, 18.0, 18.5, 19.0,
            19.5, 20.0, 21.0, 22.0, 23.0, 24.0, 25.0, 26.0, 27.0, 28.0, 29.0,
            30.0, 32.0, 34.0, 36.0, 38.0, 40.0, 42.0, 44.0, 46.0, 48.0, 50.0,
            55.0, 60.0, 65.0, 70.0, 75.0, 80.0, 85.0, 90.0, 95.0, 100.0, 110.0,
            120.0, 130.0, 140.0, 150.0, 160.0, 170.0, 180.0, 190.0, 200.0,
            210.0, 220.0, 230.0, 240.0, 250.0, 260.0, 270.0, 280.0, 290.0,
            300.0, 310.0, 320.0, 330.0, 340.0, 350.0, 360.0, 370.0, 380.0,
            390.0, 400.0, 410.0, 420.0, 430.0, 440.0, 450.0, 460.0, 470.0,
            480.0, 490.0, 500.0, 510.0, 520.0, 530.0, 540.0, 550.0, 560.0,
            570.0, 580.0, 590.0, 600.0, 610.0, 620.0, 630.0, 640.0, 650.0,
            660.0, 670.0, 680.0, 690.0, 700.0, 710.0, 720.0, 730.0, 740.0,
            750.0, 760.0, 770.0, 780.0, 790.0, 800.0, 810.0, 820.0, 830.0,
            840.0, 850.0, 860.0, 870.0, 880.0, 890.0, 900.0, 910.0, 920.0,
            930.0, 940.0, 950.0, 960.0, 970.0, 980.0, 990.0, 1000.0
        };

        public static bool IsValidPrice(double price)
        {
            return Table.Contains(price);
        }

        public static double AddPip(double price)
        {
            if (!IsValidPrice(price))
                throw new ApplicationException("Invalid Price");

            int index = Array.IndexOf<double>(Table, price);
            return Table[++index];
        }

        public static double AddPip(double price, int num)
        {
            if (!IsValidPrice(price))
                throw new ApplicationException("Invalid Price");

            int index = Array.IndexOf<double>(Table, price);
            return Table[index + num];
        }

        public static double SubtractPip(double price)
        {
            if (!IsValidPrice(price))
                throw new ApplicationException("Invalid Price");

            int index = Array.IndexOf<double>(Table, price);
            return Table[--index];
        }

        public static double SubtractPip(double price, int num)
        {
            if (!IsValidPrice(price))
                throw new ApplicationException("Invalid Price");

            int index = Array.IndexOf<double>(Table, price);
            return Table[index - num];
        }

        public static double RoundDownToNearestBetfairPrice(double price)
        {
            if (IsValidPrice(price))
                return price;

            int index = 0;
            for (int i = 0; i < Table.Length; i++)
            {
                if (Table[i] > price)
                    return Table[index];

                index++;
            }

            return 0.0;
        }

        public static double RoundUpToNearestBetfairPrice(double price)
        {
            if (IsValidPrice(price))
                return price;

            int index = 0;
            for (int i = 0; i < Table.Length; i++)
            {
                if (Table[i] > price)
                    return Table[index++];

                index++;
            }

            return 0.0;
        }

        public static double ApplySpread(double price, double percentage)
        {
            if (!IsValidPrice(price))
                throw new ApplicationException("Invalid Price");

            double adjustedPrice = price * percentage;

            if (percentage <= 1.0)
                return RoundDownToNearestBetfairPrice(adjustedPrice);
            else
                return RoundUpToNearestBetfairPrice(adjustedPrice);
        }

    }
}

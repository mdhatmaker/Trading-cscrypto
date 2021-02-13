using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Exchanges;

namespace DemoCrypto
{
    class Program
    {
        static string rootDataPath = @"C:\Users\mhatm\Downloads\data";
        static string quandlApiKey = "WLA6xivYKCt_PVBeMpbv";

        static async Task Main(string[] args)
        {
            Console.WriteLine("(Starting DemoCrypto)\n");

            //var ex1 = new EToro("{subscription key}");
            //var res1 = await ex1.MakeRequest();

            //await DemoBinance("{api key}", "BTCUSDT");

            //await DemoQuandl(quandlApiKey);
            //await DownloadQuandlDatabases(quandlApiKey);
            //await DownloadQuandlCodes(quandlApiKey, "WIKI");

            //await DemoBinanceCandlestickToCsv("{api key}", "BTCUSDT");

            //await DownloadQuandlContinuousFutures(quandlApiKey, "CME_MGC1");
            for (int timePeriod = 7; timePeriod <= 14; ++timePeriod)
            {
                CreateCsvContinuousFuturesTA("CME_MGC1", timePeriod);
                //CreateCsvCryptoTA(@"\Binance\BTCUSDT-12h-20210211-102146.csv", timePeriod);
            }

            decimal maxNet = 0;
            int maxTimePeriod = 0;
            for (int timePeriod = 14; timePeriod >= 7; --timePeriod)
            {
                var net = LoadTACsv(timePeriod, "Date", "Settle");
                //var net = LoadTACsv(timePeriod, "OpenTime", "Close");
                if (net > maxNet)
                {
                    maxNet = net;
                    maxTimePeriod = timePeriod;
                }
            }

            Console.WriteLine($"\nMAX NET: {maxNet:n2}    time periods: {maxTimePeriod}\n");

            System.Environment.Exit(0);

            //DemoCsvTA1($"{rootDataPath}\\Binance\\BTCUSDT-1d-20210208-070457.csv");
            DemoCsvTA1($"{rootDataPath}\\Binance\\BTCUSDT-5m-20210211-102143.csv");

            Console.WriteLine("\n\n(short delay)");
            Thread.Sleep(1000);
            Console.WriteLine("Done.  (Press ENTER if needed...)");
            //Console.ReadLine();
        }

        static async Task DemoQuandl(string quandlApiKey)
        {
            var qdl = new Quandl(quandlApiKey);
            //var csv = await qdl.RequestWikiStockCsv("GOOG");
            var csv = await qdl.RequestWikiContinuousFuturesCsv("CME_MGC1");
            Console.WriteLine(csv.Headers);
            foreach (var l in csv.Lines.Take(25))
            {
                Console.WriteLine(l);
            }
        }

        // where code like "CME_MGC1"
        static async Task DownloadQuandlContinuousFutures(string quandlApiKey, string code)
        {
            var qdl = new Quandl(quandlApiKey);
            var csv = await qdl.RequestWikiContinuousFuturesCsv(code);
            var filepath = Path.Join(rootDataPath, "Quandl", "ContinuousFutures", code + ".csv");
            await csv.SaveFile(filepath);
        }

        static async Task DownloadQuandlDatabases(string quandlApiKey)
        {
            var filepath = Path.Join(rootDataPath, "Quandl", "databases.txt");

            var qdl = new Quandl(quandlApiKey);
            var databases = await qdl.GetAllDatabases();
            using (var f = new StreamWriter(filepath))
            {
                await f.WriteAsync(databases);
                f.Flush();
            }
        }

        // where db like "WIKI"
        static async Task DownloadQuandlCodes(string quandlApiKey, string db)
        {
            var filepath = Path.Join(rootDataPath, "Quandl", "Codes", $"{db}.txt");

            var qdl = new Quandl(quandlApiKey);
            var codes = await qdl.GetCodes(db);
            using (var f = new StreamWriter(filepath))
            {
                await f.WriteAsync(codes);
                f.Flush();
            }
        }

        static async Task DemoBinance(string binanceApiKey, string symbol = "BTCUSDT")
        {
            var exch = new Binance(binanceApiKey);

            var ping = await exch.Ping();
            var serverTime = await exch.ServerTime();
            var info = await exch.ExchangeInfo();
            Console.WriteLine($"ping> {ping}");
            Console.WriteLine($"serverTime> {serverTime}");
            Console.WriteLine($"info> {info}");

            var ob = await exch.OrderBook(symbol, 5);
            Console.WriteLine($"\nOrderBook>\n{ob}");

            var trades = await exch.RecentTrades(symbol, 10);
            Console.WriteLine($"\nTrades>\n{trades}");
            //var tradesHist = await exch.HistoricalTrades(symbol, 15);
            //Console.WriteLine($"\nHistorical Trades:\n{trades}");
        }

        static async Task DemoBinanceCandlestickToCsv(string binanceApiKey, string symbol = "BTCUSDT")
        {
            var exch = new Binance(binanceApiKey);

            var path = Path.Join(rootDataPath, "Binance");

            Binance.CandlestickList klines;
            klines = await exch.Klines(symbol);  // default is "1m"
            await klines.OutputToCsv(path);
            klines = await exch.Klines(symbol, "3m");
            await klines.OutputToCsv(path);
            klines = await exch.Klines(symbol, "5m");
            await klines.OutputToCsv(path);
            klines = await exch.Klines(symbol, "15m");
            await klines.OutputToCsv(path);
            klines = await exch.Klines(symbol, "30m");
            await klines.OutputToCsv(path);
            klines = await exch.Klines(symbol, "1h");
            await klines.OutputToCsv(path);
            klines = await exch.Klines(symbol, "12h");
            await klines.OutputToCsv(path);
            klines = await exch.Klines(symbol, "1d");
            await klines.OutputToCsv(path);
        }

        // where timePeriod like 7
        static decimal LoadTACsv(int timePeriod = 14, string columnDate = "Date", string columnPrice = "Settle")
        {
            var filepath = $"{rootDataPath}\\ta{timePeriod}.csv";
            var csv = new Csv(new Uri(filepath));
            var date = csv.ColumnData<string>(columnDate, reverseOrder: true);
            var price = csv.ColumnData<decimal>(columnPrice, reverseOrder: true);
            var rsi = csv.ColumnData<decimal>("RSI", reverseOrder:true);
            //var li = rsi.SkipWhile(x => x == 0).ToList();
            var li = rsi;
            int dir = 0;    // direction: +1 = BUY, -1 = SELL
            decimal balance = 0;
            int tradeCount = 0;
            decimal tradeFees = 0.0m;
            decimal commissionRate = 0.003m; // 30 bips
            Console.WriteLine($"--- timePeriod = {timePeriod} ---");
            for (int i = 0; i < li.Count; ++i)
            {
                if (li[i] == 0) continue;
                if (li[i] < 30)
                {
                    if (dir != 1)
                    {
                        dir = 1;
                        Console.Write($"B {date[i]} {price[i]} {rsi[i]}  |  ");
                        balance -= price[i];
                        tradeFees += commissionRate * price[i];
                        ++tradeCount;
                    }
                }
                else if (li[i] > 70)
                {
                    if (dir != -1)
                    {
                        dir = -1;
                        Console.Write($"S {date[i]} {price[i]} {rsi[i]}  |  ");
                        balance += price[i];
                        tradeFees += commissionRate * price[i];
                        ++tradeCount;
                    }
                }
            }
            // If we don't have a balanced # of buys/sells, adjust the balance
            if (tradeCount % 2 != 0)
            {
                if (dir == 1)
                    balance += price[0];
                else
                    balance -= price[0];
            }
            var net = balance - tradeFees;
            Console.WriteLine($"\nBALANCE: {balance:n2}    ({tradeCount} trades, fees={tradeFees:n2})    NET: {net:n2}\n");
            return net;
        }

        // where code like "CME_MGC1"
        // where timePeriod like 7
        static void CreateCsvContinuousFuturesTA(string code, int timePeriod = 14)
        {
            var cfDir = $"{rootDataPath}\\Quandl\\ContinuousFutures";
            var csv = new Csv(new Uri($"{cfDir}\\{code}.csv"));
            var date = csv.ColumnData<string>("Date");
            var settle = csv.ColumnData<decimal>("Settle");

            var ta = new TechnicalAnalysis(settle);

            var rvRsi = ta.Rsi(timePeriod);
            var rsi = rvRsi.Values;

            var rvRoc = ta.Roc(timePeriod);
            var roc = rvRoc.Values;

            var rvMacd = ta.Macd();
            var macd = rvMacd.Values;

            var filepath = $"{rootDataPath}\\ta{timePeriod}.csv";
            using (var f = new StreamWriter(filepath))
            {
                f.Write("Date,Settle,RSI,ROC%,MACDHist\n");
                for (int i = 0; i < settle.Count; ++i)
                {
                    f.Write("{0},{1},{2:n2},{3:n2},{4:n2}", nullZero(date, i), nullZero(settle, i), nullZero(rsi, i), nullZero(roc, i), nullZero(macd, i));
                    if (i < settle.Count - 1)
                        f.Write("\n");
                }
                f.Flush();
            }
        }

        static T nullZero<T>(List<T> li, int i)
        {
            return i < li.Count ? li[i] : default(T);
        }

        // where fileSubPath like @"\Binance\BTCUSDT-12h-20210211-102146.csv"
        // where timePeriod like 7
        static void CreateCsvCryptoTA(string fileSubPath, int timePeriod = 14)
        {
            var csvFilepath = Path.Join($"{rootDataPath}", fileSubPath);
            var csv = new Csv(new Uri(csvFilepath));
            var time = csv.ColumnData<string>("OpenTime");
            var close = csv.ColumnData<decimal>("Close");

            var ta = new TechnicalAnalysis(close);

            var rvRsi = ta.Rsi(timePeriod);
            var rsi = rvRsi.Values;

            var rvRoc = ta.Roc(timePeriod);
            var roc = rvRoc.Values;

            var rvMacd = ta.Macd();
            var macd = rvMacd.Values;

            var filepath = $"{rootDataPath}\\ta{timePeriod}.csv";
            using (var f = new StreamWriter(filepath))
            {
                f.Write("OpenTime,Close,RSI,ROC%,MACDHist\n");
                for (int i = 0; i < close.Count; ++i)
                {
                    f.Write("{0},{1},{2:n2},{3:n2},{4:n2}", nullZero(time, i), nullZero(close, i), nullZero(rsi, i), nullZero(roc, i), nullZero(macd, i));
                    if (i < close.Count - 1)
                        f.Write("\n");
                }
                f.Flush();
            }
        }

        // where csvFilepath like @"C:\Users\mhatm\Downloads\data\BTCUSDT-1d-20210208-070457.csv"
        static void DemoCsvTA1(string csvFilepath)
        {
            // Read from historical: Prices in csv are OLDEST to NEWEST
            var csvUri = new Uri(csvFilepath);
            var hist = new Csv(csvUri, reverseOrder: true);
            Console.WriteLine(hist.Headers);
            //var str = hist["Close"];
            //Console.WriteLine("{0} {1} {2}", str[0], str[1], str[2]);
            var closes = hist.ColumnData<decimal>("Close");
            var highs = hist.ColumnData<decimal>("High");
            var lows = hist.ColumnData<decimal>("Low");
            //var closes = new List<decimal>() { 109, 108, 107, 106, 105, 104, 103, 102, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120 };

            //PrintColumnData(closes);

            //var ta = new TechnicalAnalysis(closes);
            var ta = new TechnicalAnalysis(closes, highs, lows);

            var rvRsi = ta.Rsi(14);
            var rsi = rvRsi.Values;

            var rvRoc = ta.Roc(14);
            var roc = rvRoc.Values;

            var rvMacd = ta.Macd();
            var macd = rvMacd.Values;

            for (int i = 0; i < closes.Count; ++i)
                Console.WriteLine("{0}    {1:n2}    {2:n2} %    {3:n2}", i < closes.Count ? closes[i] : 0, i < rsi.Count ? rsi[i] : 0, i < roc.Count ? roc[i] : 0, i < macd.Count ? macd[i] : 0);
        }

        // Use this to copy/paste into calculators like https://www.marketvolume.com/quotes/calculatersi.asp
        static void PrintColumnData(List<decimal> li, bool reverseOrder = false)
        {
            List<decimal> prices;
            if (reverseOrder)
                prices = li.Skip(0).Take(li.Count).Reverse().ToList<decimal>();
            else
                prices = li;

            foreach (var p in prices)
                Console.Write("{0}, ", p);
            Console.WriteLine();
        }


    } // end of class
} // end of namespace

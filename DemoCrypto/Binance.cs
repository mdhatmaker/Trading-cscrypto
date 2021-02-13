using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.IO;
using ExtensionMethods;

namespace Exchanges
{
    //using AsyncResponse = Task<HttpResponseMessage>;

    public class Binance
    {
        private string apiKey = "{subscription key}";
        private string endpoint = "https://api.binance.com";
        //private string endpoint = "https://api1.binance.com";
        //private string endpoint = "https://api2.binance.com";
        //private string endpoint = "https://api3.binance.com";

        public Binance(string key)
        {
            apiKey = key;
        }

        public async Task<HttpResponseMessage> MakeRequest(string route = "/api/v3/ping", string query = "")
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(query);   // "key=value&key=value&key=value..."

            // Request headers
            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

            var uri = endpoint + route + "?" + queryString;

            var response = await client.GetAsync(uri);
            return response;
        }

        private async Task<string> ContentString(HttpResponseMessage response)
        {
            string result = null;

            if (response.Content is object)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return result ?? String.Empty;
        }

        public async Task<T> ContentJson<T>(string route, string query = "")
        {
            var response = await MakeRequest(route, query);
            var json = await ContentString(response);
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        //-----------------------------------------------------------------------------------------

        public async Task<JPing> Ping()
        {
            return await ContentJson<JPing>("/api/v3/ping");
        }

        public async Task<JServerTime> ServerTime()
        {
            return await ContentJson<JServerTime>("/api/v3/time");
        }

        public async Task<JExchangeInfo> ExchangeInfo()
        {
            return await ContentJson<JExchangeInfo>("/api/v3/exchangeInfo");
        }

        //-----------------------------------------------------------------------------------------

        public async Task<JDepth> OrderBook(string symbol, int limit = 100)
        {
            // symbol   STRING
            // limit    INT     Default 100; max 5000. Valid limits:[5, 10, 20, 50, 100, 500, 1000, 5000]
            string query = string.Format($"symbol={symbol}&limit={limit}");
            return await ContentJson<JDepth>("/api/v3/depth", query);
        }

        public async Task<JTrades> RecentTrades(string symbol, int limit = 500)
        {
            // symbol   STRING
            // limit    INT     Default 500; max 1000.
            string query = string.Format($"symbol={symbol}&limit={limit}");
            return await ContentJson<JTrades>("/api/v3/trades", query);
        }

        public async Task<JTrades> HistoricalTrades(string symbol, int limit = 500, long fromTradeId = -1L)
        {
            // symbol   STRING
            // limit    INT     Default 500; max 1000.
            // fromId	LONG	Trade id to fetch from. Default gets most recent trades.
            string query = string.Format($"symbol={symbol}&limit={limit}");
            if (fromTradeId >= 0)
                query += $"&fromId={fromTradeId}";
            return await ContentJson<JTrades>("/api/v3/historicalTrades", query);
        }


        //-----------------------------------------------------------------------------------------

        public async Task<CandlestickList> Klines(string symbol, string interval = "1m", int limit = 500, long startTime = -1L, long endTime = -1L)
        {
            // symbol       STRING
            // interval     ENUM    1m,3m,5m,15m,30m,1h,2h,4h,6h,8h,12h,1d,3d,1w,1M  (m->minutes; h->hours; d->days; w->weeks; M->months)
            // startTime    LONG    If startTime and endTime are not sent, 
            // endTime      LONG      the most recent klines are returned.
            // limit        INT     Default 500; max 1000.
            string query = string.Format($"symbol={symbol}&interval={interval}&limit={limit}");
            if (startTime >= 0)
                query += $"&startTime={startTime}";
            if (endTime >= 0)
                query += $"&endTime={endTime}";
            var jarr = await ContentJson<JArray>("/api/v3/klines", query);
            var candlesticks = new CandlestickList(symbol, interval);
            for (int i = 0; i < jarr.Count(); ++i)
            {
                candlesticks.Add(new Candlestick(jarr[i]));
            }
            return candlesticks;
        }

        public class Candlestick
        {
            public long openTime { get; private set; }
            public decimal open { get; private set; }
            public decimal high { get; private set; }
            public decimal low { get; private set; }
            public decimal close { get; private set; }
            public decimal volume { get; private set; }
            public long closeTime { get; private set; }
            public decimal quoteAssetVolume { get; private set; }
            public int numberOfTrades { get; private set; }
            public decimal takerBuyBaseAssetVolume { get; private set; }
            public decimal takerBuyQuoteAssetVolume { get; private set; }
            public decimal ignore { get; private set; }

            public Candlestick(JToken klarr)
            {
                openTime = klarr[0].ToObject<long>();
                open = klarr[1].ToObject<decimal>();
                high = klarr[2].ToObject<decimal>();
                low = klarr[3].ToObject<decimal>();
                close = klarr[4].ToObject<decimal>();
                volume = klarr[5].ToObject<decimal>();
                closeTime = klarr[6].ToObject<long>();
                quoteAssetVolume = klarr[7].ToObject<decimal>();
                numberOfTrades = klarr[8].ToObject<int>();
                takerBuyBaseAssetVolume = klarr[9].ToObject<decimal>();
                takerBuyQuoteAssetVolume = klarr[10].ToObject<decimal>();
                ignore = klarr[11].ToObject<decimal>();
            }

            public static string ColumnHeaders = "OpenTime,Open,High,Low,Close,Volume,CloseTime,QuoteAssetVolume,NumberOfTrades,TakerBuyBaseAssetVolume,TakerBuyQuoteAssetVolume";

            public override string ToString()
            {
                return string.Format($"{openTime},{open},{high},{low},{close},{volume},{closeTime},{quoteAssetVolume},{numberOfTrades},{takerBuyBaseAssetVolume},{takerBuyQuoteAssetVolume}");
            }
        }

        public class CandlestickList : List<Candlestick>
        {
            public string Symbol { get; private set; }
            public string Interval { get; private set; }

            public CandlestickList(string symbol, string interval) : base()
            {
                this.Symbol = symbol;
                this.Interval = interval;
            }

            // Given a path, write the candlestick data to a .csv file named using the symbol, interval, date, and time.
            public async Task OutputToCsv(string path)
            {
                var now = DateTime.Now;
                var filename = string.Format($"{Symbol}-{Interval}-{now.ToString("yyyyMMdd")}-{now.ToString("hhmmss")}.csv");
                var filepath = Path.Join(path, filename);
                using (var f = new StreamWriter(filepath))
                {
                    await f.WriteLineAsync(Candlestick.ColumnHeaders);
                    foreach (var cstick in this)
                    {
                        await f.WriteLineAsync(cstick.ToString());
                    }
                    await f.FlushAsync();
                }
            }
        }


        //-----------------------------------------------------------------------------------------


    } // class

    public class JPing
    {
        // empty JSON response

        public override string ToString()
        {
            return "(ping)";
        }
    }

    public class JServerTime
    {
        public long serverTime { get; set; }

        public override string ToString()
        {
            return $"{serverTime.LocalTimeStr()}";
        }
    }

    public class JExchangeInfo
    {
        public string timezone { get; set; }
        public long serverTime { get; set; }
        public IList<RateLimit> rateLimits { get; set; }
        public IList<JObject> exchangeFilters { get; set; }     // IList<Filter>
        public IList<BinanceSymbol> symbols { get; set; }

        public override string ToString()
        {
            return $"tz:{timezone}   time:{serverTime.LocalTimeStr()}   #rateLimits:{rateLimits.Count}  #exchangeFilters:{exchangeFilters.Count}   #symbols:{symbols.Count}";
        }
    }

    public class BinanceSymbol
    {
        public string symbol { get; set; }
        public string status { get; set; }
        public string baseAsset { get; set; }
        public int baseAssetPrecision { get; set; }
        public string quoteAsset { get; set; }
        public int quoteAssetPrecision { get; set; }
        public int baseCommissionPrecision { get; set; }
        public int quoteCommissionPrecision { get; set; }
        public IList<string> orderTypes { get; set; }
        public bool icebergAllowed { get; set; }
        public bool ocoAllowed { get; set; }
        public bool quoteOrderQtyMarketAllowed { get; set; }
        public bool isSpotTradingAllowed { get; set; }
        public bool isMarginTradingAllowed { get; set; }
        public IList<JObject> filters { get; set; }     // IList<Filter>
        public IList<string> permissions { get; set; }
    }

    /*public class Filter : JObject
    {

    }*/

        public class RateLimit
    {
        public string rateLimitType { get; set; }   // "REQUEST_WEIGHT", "ORDERS", "RAW_REQUESTS"
        public string interval { get; set; }        // "SECOND", "MINUTE", "DAY"
        public int intervalNum { get; set; }
        public int limit { get; set; }
    }

    public class JDepth
    {
        public long lastUpdateId { get; set; }
        public Depth bids { get; set; }
        public Depth asks { get; set; }

        public override string ToString()
        {
            return "---asks---\n" + this.asks.ToString() + "\n---bids---\n" + this.bids.ToString();
        }
    }

    /*public class DepthEntry : IList<decimal>
    {
        public IEnumerator<decimal> GetEnumerator() { return li.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return li.AsEnumerable<decimal>().GetEnumerator(); }
    }*/

    public class DepthEntry : List<decimal>
    {
        public decimal Price => this[0];
        public decimal Qty => this[1];

        public override string ToString()
        {
            return string.Format($"{Price} x {Qty}");
        }
    }

    public class Depth : List<DepthEntry>
    {
        public DepthEntry InsideMarket => this[0];

        public override string ToString()
        {
            return string.Join('|', this);
        }
    }

    public class JTrades : List<Trade>
    {
        public override string ToString()
        {
            return string.Join('\n', this);
        }
    }

    public class Trade
    {
        public long id { get; set; }
        public decimal price { get; set; }
        public decimal qty { get; set; }
        public decimal quoteQty { get; set; }
        public long time { get; set; }
        public bool isBuyerMaker { get; set; }
        public bool isBestMatch { get; set; }

        //public DateTimeOffset Time => DateTimeOffset.FromUnixTimeMilliseconds(time);
        //public string ShortTime => Time.DateTime.ToShortDateString() + " " + Time.DateTime.ToShortTimeString();
        //public string LocalTime => Time.DateTime.ToLocalTime().ToShortDateString() + " " + Time.DateTime.ToLocalTime().ToShortTimeString();

        public override string ToString()
        {
            return $"[{id}]   prc:{price}  qty:{qty}  quoteQty:{quoteQty}  time:{time.LocalTimeStr()}  isBuyerMaker:{isBuyerMaker}  isBestMatch:{isBestMatch}";
        }
    }

    /*public class Account
    {
        public string Email { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public IList<string> Roles { get; set; }
    }*/


    /*  Spot API URL                            Spot Test Network URL
        https://api.binance.com/api	            https://testnet.binance.vision/api
        wss://stream.binance.com:9443/ws        wss://testnet.binance.vision/ws
        wss://stream.binance.com:9443/stream    wss://testnet.binance.vision/stream */


} // namespace



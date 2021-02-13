using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DemoCrypto
{
    // https://blog.quandl.com/getting-started-with-the-quandl-api

    public class Quandl
    {
        string endpoint = "https://www.quandl.com/api/v3";
        string apikey;

        public Quandl(string apikey)
        {
            this.apikey = apikey;
        }

        // where stockSymbol like "FB"
        public async Task<Csv> RequestWikiStockCsv(string stockSymbol)
        {
            return await RequestCsv($"/datasets/WIKI/{stockSymbol}/data.csv");
        }

        // where continuousSymbol like "CME_MGC1"
        public async Task<Csv> RequestWikiContinuousFuturesCsv(string continuousSymbol)
        {
            return await RequestCsv($"/datasets/CHRIS/{continuousSymbol}.csv");
        }

        // where csvRoute like "/datasets/WIKI/FB/data.csv"
        public async Task<Csv> RequestCsv(string csvRoute)
        {
            // https://www.quandl.com/api/v3/datasets/WIKI/FB/data.csv?api_key=YOURAPIKEYHERE
            // https://www.quandl.com/api/v3/datasets/WIKI/FB/data.csv?column_index=4&exclude_column_names=true&rows=3&start_date=2012-11-01&end_date=2013-11-30&order=asc&collapse=quarterly&transform=rdiff
            //var url = $"/datasets/WIKI/FB/data.csv?api_key={this.apikey}";
            var rm = await MakeRequest(csvRoute);
            var content = await rm.Content.ReadAsStringAsync();
            return new Csv(content);
        }

        public async Task<HttpResponseMessage> MakeRequest(string qid, string query = "")
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(query);  //string.Empty);

            // Request headers
            //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apikey);

            var uri = endpoint + $"{qid}?api_key={this.apikey}" + queryString;

            var response = await client.GetAsync(uri);
            return response;
        }

        // Generic Quandl request method
        public async Task<HttpResponseMessage> RequestUrl(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url + "?api_key={this.apikey}");
            return response;
        }

        // Get list of all Quandl databases
        public async Task<string> GetAllDatabases()
        {
            var response = await RequestUrl(endpoint + "/databases");
            return await response.Content.ReadAsStringAsync();
        }

        // where db like "WIKI"
        public async Task<string> GetCodes(string db)
        {
            var url = endpoint + $"/databases/{db}/codes.json";
            var response = await RequestUrl(url);
            return await response.Content.ReadAsStringAsync();
        }

    } // class
} // namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Threading.Tasks;

namespace Exchanges
{
    public class EToro
    {
        private string apiKey = "{subscription key}";
        private string endpoint = "https://api.etoro.com";

        public EToro(string key)
        {
            apiKey = key;
        }

        public async Task<HttpResponseMessage> MakeRequest()
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            var uri = endpoint + "/Metadata/V1/AssetClasses?" + queryString;

            var response = await client.GetAsync(uri);
            return response;
        }

    } // class

} // namespace



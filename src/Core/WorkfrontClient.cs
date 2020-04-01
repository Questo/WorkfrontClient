using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core
{
    public class WorkfrontClient : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;

        public WorkfrontClient(string endpoint, string apiKey)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(endpoint)
            };

            _apiKey = apiKey;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        /// <summary>
        /// Creates a new object of the given type
        /// </summary>
        /// <param name="objCode">The object code of the object to create</param>
        /// <param name="parameters">Additional parameters to be included.
        /// <para />Depending on the object type certain parameters are required.</param>
        /// <returns>A <see chref="JToken" /></returns>
        public async Task<JToken> Create(string objCode, Dictionary<string, string> parameters)
        {
            string[] paramArray = ParamDictionaryToStringArray(parameters, $"apiKey={this._apiKey}");
            string queryString = ToQueryString(paramArray);
            string requestUri = @$"{_apiPath}/{objCode}/{queryString}";

            var response = await _client.PostAsync(requestUri, null);

            if (!response.IsSuccessStatusCode)
                throw new WorkfrontException((int)response.StatusCode, response.ReasonPhrase);

            string responseBody = await response.Content.ReadAsStringAsync();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseBody);

            return json.Value<JToken>("data");
        }

        /// <summary>
        /// Gets the object of the given object code and the given id.
        /// </summary>
        /// <param name="objCode">A <see cref="String" /> representing the type of object you are getting</param>
        /// <param name="id">A <see cref="String" /> representing the ID of object you are getting</param>
        /// <param name="fieldsToInclude">Additional parameters to be included.
        /// <para />Depending on the object type certain parameters are required.</param>
        /// <returns>A <see chref="JToken" /></returns>
        public async Task<JToken> Get(string objCode, string id, params string[] fieldsToInclude)
        {
            var parameters = new List<string>
            {
                $"apiKey={_apiKey}"
            };
            var sb = new StringBuilder();

            if (fieldsToInclude != null && fieldsToInclude.Length > 0)
            {
                fieldsToInclude.ToList().ForEach(s => sb.Append(s).Append(","));
                sb.Remove(sb.Length - 1, 1);
                parameters.Add($"fields={sb.ToString()}");
            }

            string queryString = ToQueryString(parameters.ToArray());
            string requestUri = @$"{_apiPath}/{objCode}/{id}/{queryString}";

            var response = await _client.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                throw new WorkfrontException((int)response.StatusCode, response.ReasonPhrase);

            string responseBody = await response.Content.ReadAsStringAsync();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseBody);

            return json.Value<JToken>("data");
        }

        /// <summary>
        /// Searches on the given object code.
        /// </summary>
        /// <param name="objCode">A <see cref="String" /> representing the type of object you are searching for</param>
        /// <param name="parameters">A <see cref="Dictionary{String, String}" /> with all the parameters to include in the search</param>
        /// <returns>A <see cref="JToken" /></returns>
        public async Task<JToken> Search(string objCode, Dictionary<string, string> parameters)
        {
            string[] paramsArray = ParamDictionaryToStringArray(parameters, $"apiKey={_apiKey}");
            string queryString = ToQueryString(paramsArray);
            string requestUri = @$"{_apiPath}/{objCode}/search{queryString}";

            var response = await _client.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                throw new WorkfrontException((int)response.StatusCode, response.ReasonPhrase);

            string responseBody = await response.Content.ReadAsStringAsync();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseBody);

            return json.Value<JToken>("data");
        }

        /// <summary>
        /// Updates an object that already exists.
        /// </summary>
        /// <param name="objCode">A <see cref="String" /> representing the type of object to update</param>
        /// <param name="id">A <see cref="String" /> representing the ID of object to update</param>
        /// <param name="parameters">Additional parameters of the object to update</param>
        /// <returns>A <see cref="JToken" /></returns>
        public async Task<JToken> Update(string objCode, string id, Dictionary<string, string> parameters)
        {
            string[] paramsArray = ParamDictionaryToStringArray(parameters, $"apiKey={_apiKey}");
            string queryString = ToQueryString(paramsArray);
            string requestUri = @$"{_apiPath}/{objCode}/{id}{queryString}";

            var response = await _client.PutAsync(requestUri, null);

            if (!response.IsSuccessStatusCode)
                throw new WorkfrontException((int)response.StatusCode, response.ReasonPhrase);

            string responseBody = await response.Content.ReadAsStringAsync();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseBody);

            return json.Value<JToken>("data");
        }

        public async Task<JToken> Update(string objCode, object updates)
        {
            string queryString = $"updates={JsonConvert.SerializeObject(updates)}";
            string requestUri = $@"{_apiPath}{objCode}?apiKey={_apiKey}&{queryString}";

            var response = await _client.PutAsync(requestUri, null);

            if (!response.IsSuccessStatusCode)
                throw new WorkfrontException((int)response.StatusCode, response.ReasonPhrase);

            string responseBody = await response.Content.ReadAsStringAsync();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseBody);

            return json.Value<JToken>("data");
        }


        private string[] ParamDictionaryToStringArray(Dictionary<string, string> parameters, params string[] toAdd)
        {
            var paramList = new List<string>();
            paramList.AddRange(toAdd);

            foreach (var item in parameters)
            {
                string line;
                line = $"{item.Key}={item.Value.ToString()}";
                paramList.Add(line);
            }

            return paramList.ToArray();
        }

        private string ToQueryString(string[] parameters)
        {
            var sb = new StringBuilder();
            parameters.ToList().ForEach(s => sb.Append(s).Append("&"));
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return $"?{sb.ToString()}";
        }
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ovicus.ShowMyHomework
{
    public class ShowMyHomeworkClient
    {
        private const string ApiBaseUrl = "https://api.showmyhomework.co.uk";
        private const string AcceptHeaderValue = "application/smhw.v3+json";

        private readonly HttpClient _client;

        public ShowMyHomeworkClient(string accessToken) : this(accessToken, ApiBaseUrl, new HttpClient())
        {

        }

        public ShowMyHomeworkClient(string accessToken, string endpointBase) : this(accessToken, endpointBase, new HttpClient())
        {
            
        }

        public ShowMyHomeworkClient(string accessToken, string endpointBase, HttpClient client)
        {
            client = client ?? new HttpClient();
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(endpointBase, UriKind.Absolute);
            }

            if (client.DefaultRequestHeaders.Authorization == null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AcceptHeaderValue));

            _client = client;
        }

        public async Task<IEnumerable<Todo>> GetTodos(bool includeUncompleted = false)
        {
            var response = await _client.GetAsync("/api/todos");
            var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            JObject root = JObject.Parse(json);
            var todos = root.SelectToken("todos").ToObject<Todo[]>();
            return todos.Where(todo => !todo.Completed || includeUncompleted)
                        .OrderBy(todo => todo.DueOn);
        }
    }
}

using Newtonsoft.Json;
using Ovicus.ShowMyHomework.Auth.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ovicus.ShowMyHomework.Auth
{
    public interface IAuthenticationService
    {
        Task<string> GetAccessToken();
        Task<bool> Authenticate(string username, string password, string schoolId);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private const string ApiBaseUrl = "https://api.showmyhomework.co.uk";
        private const string AcceptHeaderValue = "application/smhw.v3+json";
        private const string OAuthClientId = "55283c8c45d97ffd88eb9f87e13f390675c75d22b4f2085f43b0d7355c1f";
        private const string OAuthClientSecret = "c8f7d8fcd0746adc50278bc89ed6f004402acbbf4335d3cb12d6ac6497d3";

        private readonly HttpClient _client;
        private AuthToken _currentToken;

        public AuthenticationService(string endpointBase, HttpClient client)
        {
            client = client ?? new HttpClient();
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(endpointBase, UriKind.Absolute);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AcceptHeaderValue));

            _client = client;
        }
        
        public AuthenticationService() : this(ApiBaseUrl, new HttpClient())
        {
        }

        public async Task<bool> Authenticate(string username, string password, string schoolId)
        {
            // Get new token with grant_type=password
            var requestUrl = $"/oauth/token?client_id={OAuthClientId}&client_secret={OAuthClientSecret}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));
            keyValues.Add(new KeyValuePair<string, string>("school_id", schoolId));
            keyValues.Add(new KeyValuePair<string, string>("username", username));
            keyValues.Add(new KeyValuePair<string, string>("password", password));

            request.Content = new FormUrlEncodedContent(keyValues);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            _currentToken = JsonConvert.DeserializeObject<AuthToken>(json);

            return _currentToken != null;
        }

        public async Task<string> GetAccessToken()
        {
            if (_currentToken == null)
            {
                throw new InvalidOperationException("Token not found. Call Authenticate() to generate a new token");
            }

            if (_currentToken.IsExpired())
            {
                _currentToken = await RefreshToken();
            }

            return _currentToken.AccessToken;
        }

        private async Task<AuthToken> RefreshToken()
        {
            // Get new token with grant_type=refresh_token
            var requestUrl = $"/oauth/token?client_id={OAuthClientId}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            keyValues.Add(new KeyValuePair<string, string>("refresh_token", _currentToken.RefreshToken));

            request.Content = new FormUrlEncodedContent(keyValues);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _client.SendAsync(request);
            var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            _currentToken = JsonConvert.DeserializeObject<AuthToken>(json);

            return _currentToken;
        }
    }
}
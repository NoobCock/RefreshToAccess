using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RefreshToAccess
{

    internal class MSLogin
    {

        public static MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        public static HttpClient _httpClient = new HttpClient();

        public static async Task<string[]> RequestTokenAsync(string token, ClientIdentification clientIdentification)
        {
            mainWindow.Indicator.Content="Getting Microsoft Token...";
            string microsoftTokenAndRefreshToken = await GetMicrosoftTokenFromRefreshTokenAsync(token, clientIdentification.Scope, clientIdentification.ClientId);
            mainWindow.Indicator.Content="Getting Xbox Live Token...";
            string xBoxLiveToken = await XboxTokenAuthAsync(microsoftTokenAndRefreshToken, clientIdentification != ClientIdentification.Vanilla);
            mainWindow.Indicator.Content="Getting Xbox User Hash...";
            string[] xstsTokenAndHash = await XboxUserHashAsync(xBoxLiveToken);
            mainWindow.Indicator.Content="Getting Access Token...";
            string accessToken = await GetAccessTokenAsync(xstsTokenAndHash[1], xstsTokenAndHash[0]);
            mainWindow.Indicator.Content="Getting Player Profile...";
            string[] profile = await GetProfileAsync(accessToken);
            return new string[] { profile[1], profile[0], accessToken };
        }

        private static async Task<string> GetMicrosoftTokenFromRefreshTokenAsync(string refreshToken, string scope, string clientId)
        {
            const string url = "https://login.live.com/oauth20_token.srf";
            var parameters = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "refresh_token", refreshToken },
                { "grant_type", "refresh_token" },
                { "redirect_uri", "https://login.live.com/oauth20_desktop.srf" },
                { "scope", scope }
            };

            var content = new FormUrlEncodedContent(parameters);
            HttpResponseMessage httpResponse = await _httpClient.PostAsync(url, content);
            httpResponse.EnsureSuccessStatusCode();
            string responseBody = await httpResponse.Content.ReadAsStringAsync();
            var resp = JObject.Parse(responseBody);

            return resp["access_token"].ToString();
        }

        private static async Task<string> XboxTokenAuthAsync(string authToken, bool direct)
        {
            const string url = "https://user.auth.xboxlive.com/user/authenticate";

            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" }
            };

            var requestProps = new JObject
            {
                { "AuthMethod", "RPS" },
                { "SiteName", "user.auth.xboxlive.com" },
                { "RpsTicket", (direct ? "d=" : "") + authToken }
            };

            var request = new JObject
            {
                { "Properties", requestProps },
                { "RelyingParty", "http://auth.xboxlive.com" },
                { "TokenType", "JWT" }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(request.ToString(), Encoding.UTF8, "application/json")
            };

            foreach (var header in headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var resp = JObject.Parse(responseContent);

                return resp["Token"]?.ToString()
                    ?? throw new InvalidOperationException("Token not found in Xbox authentication response");
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Xbox token authentication failed: {ex.Message}", ex);
            }
        }

        private static async Task<string[]> XboxUserHashAsync(string xboxToken)
        {
            const string url = "https://xsts.auth.xboxlive.com/xsts/authorize";
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Accept", "application/json" }
            };

            var userTokens = new JArray { xboxToken };
            var requestProps = new JObject
            {
                { "UserTokens", userTokens },
                { "SandboxId", "RETAIL" }
            };

            var request = new JObject
            {
                { "Properties", requestProps },
                { "RelyingParty", "rp://api.minecraftservices.com/" },
                { "TokenType", "JWT" }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(request.ToString(), Encoding.UTF8, "application/json")
            };

            foreach (var header in headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var resp = JObject.Parse(responseContent);

            string token = resp["Token"].ToString();
            string userHash = resp["DisplayClaims"]["xui"][0]["uhs"].ToString();
            return new string[] { token, userHash };
        }

        private static async Task<string> GetAccessTokenAsync(string userHash, string xstsToken)
        {
            const string url = "https://api.minecraftservices.com/authentication/login_with_xbox";
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };

            var request = new JObject
            {
                { "identityToken", $"XBL3.0 x={userHash};{xstsToken}" }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(request.ToString(), Encoding.UTF8, "application/json")
            };

            foreach (var header in headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var resp = JObject.Parse(responseContent);

            return resp["access_token"].ToString();
        }

        private static async Task<string[]> GetProfileAsync(string accessToken)
        {
            const string url = "https://api.minecraftservices.com/minecraft/profile";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var resp = JObject.Parse(responseContent);

            return new string[] { resp["id"].ToString(), resp["name"].ToString() };
        }
    }

    public class ClientIdentification
    {
        public ClientIdentification(string clientId, string scope)
        {
            this.ClientId=clientId;
            this.Scope=scope;
        }


        public static ClientIdentification Vanilla = new ClientIdentification("00000000402b5328", "service::user.auth.xboxlive.com::MBI_SSL");
        public static ClientIdentification HMCL = new ClientIdentification("6a3728d6-27a3-4180-99bb-479895b8f88e", "XboxLive.signin offline_access");
        public static ClientIdentification PCL = new ClientIdentification("fe72edc2-3a6f-4280-90e8-e2beb64ce7e1", "XboxLive.signin offline_access");
        public static ClientIdentification essential = new ClientIdentification("e39cc675-eb52-4475-b5f8-82aaae14eeba", "Xboxlive.signin Xboxlive.offline_access");

        public string Scope { get; }
        public string ClientId { get; }
    }
}

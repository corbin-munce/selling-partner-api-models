using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Amazon.SellingPartnerAPIAA
{
    public class LWAClient
    {
        public const string AccessTokenKey = "access_token";
        public const string JsonMediaType = "application/json";

        public RestClient restClient { get; set; }
        public RestClientOptions restClientOptions { get; set; }
        public LWAAccessTokenRequestMetaBuilder LWAAccessTokenRequestMetaBuilder { get; set; }
        public LWAAuthorizationCredentials LWAAuthorizationCredentials { get; private set; }


        public LWAClient(LWAAuthorizationCredentials lwaAuthorizationCredentials)
        {
            LWAAuthorizationCredentials = lwaAuthorizationCredentials;
            LWAAccessTokenRequestMetaBuilder = new LWAAccessTokenRequestMetaBuilder();
            restClientOptions = new RestClientOptions(LWAAuthorizationCredentials.Endpoint.GetLeftPart(System.UriPartial.Authority));
            restClient = new RestClient(restClientOptions);
        }

        /// <summary>
        /// Retrieves access token from LWA
        /// </summary>
        /// <param name="lwaAccessTokenRequestMeta">LWA AccessTokenRequest metadata</param>
        /// <returns>LWA Access Token</returns>
        public async virtual Task<string> GetAccessToken()
        {
            LWAAccessTokenRequestMeta lwaAccessTokenRequestMeta = LWAAccessTokenRequestMetaBuilder.Build(LWAAuthorizationCredentials);
            var accessTokenRequest = new RestRequest(LWAAuthorizationCredentials.Endpoint.AbsolutePath, Method.Post);
            string jsonRequestBody = JsonConvert.SerializeObject(lwaAccessTokenRequestMeta);
            accessTokenRequest.AddBody(jsonRequestBody, JsonMediaType);
            string accessToken;
            try
            {
                var response = await restClient.ExecuteAsync(accessTokenRequest);
                if (!IsSuccessful(response))
                {
                    throw new IOException("Unsuccessful LWA token exchange", response.ErrorException);
                }

                JObject responseJson = JObject.Parse(response.Content);

                accessToken = responseJson.GetValue(AccessTokenKey).ToString();
            }
            catch (Exception e)
            {
                throw new SystemException("Error getting LWA Access Token", e);
            }
            return accessToken;
        }

        private bool IsSuccessful(RestResponse response)
        {
            int statusCode = (int)response.StatusCode;
            return statusCode >= 200 && statusCode <= 299 && response.ResponseStatus == ResponseStatus.Completed;
        }
    }
}

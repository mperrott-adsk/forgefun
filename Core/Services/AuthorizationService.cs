using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ForgeFun.Core.Services
{
    public class AuthorizationService
    {
      
        public string GenerateToken(string clientId, string clientSecret, IEnumerable<string> scopes)
        {

            var formatedScopes = string.Join(" ", scopes);

            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest("/authentication/v1/authenticate", Method.POST);

            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", formatedScopes);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic dynamicResponse = JObject.Parse(response.Content);
                return dynamicResponse.access_token;
            }

            throw new NotImplementedException();
        }

    }


}

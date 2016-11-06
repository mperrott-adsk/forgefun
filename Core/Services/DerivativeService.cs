using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ForgeFun.Core.Services
{
    public class DerivativeService
    {
        private string _token;

        public void SetToken(string token)
        {
            _token = token;
        }

        public void ConvertToSvf(string objectId, string[] views)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest("/modelderivative/v2/designdata/job", Method.POST);

            var base64Urn = Base64EncodeObjectId(objectId);

            request.AddHeader("Authorization", $"Bearer {_token}");
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(
                new
                {
                    input = new
                    {
                        urn = base64Urn
                    },
                    output = new
                    {
                        formats = new[] 
                        {
                            new
                            {
                                type = "svf", views
                            }
                        }

                    },
                });


            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }
        }

        public static string Base64EncodeObjectId(string plainText)
        {
            
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            var s = System.Convert.ToBase64String(plainTextBytes);

            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        public IEnumerable<Derivative> GetDerivatives(string objectId)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var base64Urn = Base64EncodeObjectId(objectId);

            var request = new RestRequest($"/modelderivative/v2/designdata/{base64Urn}/manifest", Method.GET);

            request.AddHeader("Authorization", $"Bearer {_token}");

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }

            dynamic dynamicJson = JObject.Parse(response.Content);


            var derivatives = new List<Derivative>();
            foreach (var derivative in dynamicJson.derivatives)
            {
                if (derivative.name != null)
                {
                    derivatives.Add(new Derivative(derivative.name.Value, derivative.status.Value,
                        derivative.outputType.Value));
                }
                
            }

            return derivatives;
        }

        public int DerivativesPercentComplete(string objectId)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var base64Urn = Base64EncodeObjectId(objectId);

            var request = new RestRequest($"/modelderivative/v2/designdata/{base64Urn}/manifest", Method.GET);

            request.AddHeader("Authorization", $"Bearer {_token}");

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return -1;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }

            dynamic dynamicJson = JObject.Parse(response.Content);


            string progressString = dynamicJson.progress;

            if (progressString == null)
            {
                return -1;
            }
            else if (progressString == "complete")
            {
                return 100;
            }
            else if (progressString.Contains("%"))
            {
                var percentIndex = progressString.IndexOf("%", StringComparison.InvariantCulture);
                var percentageString = progressString.Substring(0, percentIndex);
                var percentage = int.Parse(percentageString);
                return percentage;
            }
            else
            {
                throw new Exception("Unexpected Status");
            }

        }
    }

    public class Derivative
    {
        public Derivative(string name, string status, string outputType)
        {
            Name = name;
            Status = status;
            OutputType = outputType;
        }

        public string Name { get;  private set; }
        public string Status { get; private set; }
        public string OutputType { get; private set; }
    }
}

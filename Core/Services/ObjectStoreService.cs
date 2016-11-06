using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions;

namespace ForgeFun.Core.Services
{
    public class ObjectStoreService
    {
        private string _token;

        public void SetToken(string token)
        {
            _token = token;
        }

        public string CreateBucket(string bucketKey)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest("/oss/v2/buckets", Method.POST);

            request.AddHeader("Authorization", $"Bearer {_token}");
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(
                new
                {
                    policyKey = "transient",
                    bucketKey = bucketKey,
                });


            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }

            return null;
        }

        public IEnumerable<string> ListBuckets()
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest("/oss/v2/buckets", Method.GET);

            request.AddHeader("Authorization", $"Bearer {_token}");


            var response = client.Execute(request);
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }

            dynamic dynamicJson = JObject.Parse(response.Content);

            var bucketKeys = new List<string>();
            foreach (var item in dynamicJson.items)
            {
                bucketKeys.Add(item.bucketKey.Value);
            }

            return bucketKeys;
        }

        public IEnumerable<string> ListBucketContents(string bucketKey)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest($"/oss/v2/buckets/{bucketKey}/objects", Method.GET);

            request.AddHeader("Authorization", $"Bearer {_token}");


            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }

            dynamic dynamicJson = JObject.Parse(response.Content);

            var bucketKeys = new List<string>();
            foreach (var item in dynamicJson.items)
            {
                bucketKeys.Add(item.objectKey.Value);
            }

            return bucketKeys;
        }

        public void UploadFile(string bucketKey, string fileName, string filePath)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest($"/oss/v2/buckets/{bucketKey}/objects/{fileName}", Method.PUT);

            var bytes = File.ReadAllBytes(filePath);

            request.AddHeader("Authorization", $"Bearer {_token}");
            request.AddParameter("application/octet-stream", bytes, ParameterType.RequestBody);
            
            

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }
        }

        public string GetFileObjectId(string bucketKey, string fileName)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest($"/oss/v2/buckets/{bucketKey}/objects/{fileName}/details", Method.GET);

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

            return dynamicJson.objectId;
        }

        public void DeleteFile(string bucketKey, string fileName)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest($"/oss/v2/buckets/{bucketKey}/objects/{fileName}", Method.DELETE);

            request.AddHeader("Authorization", $"Bearer {_token}");

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("File Not Found");
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request Failed!");
            }
        }

        public void DownloadFile(string bucketKey, string fileName, string downloadTo)
        {
            var client = new RestClient("https://developer.api.autodesk.com");

            var request = new RestRequest($"/oss/v2/buckets/{bucketKey}/objects/{fileName}", Method.GET);

            request.AddHeader("Authorization", $"Bearer {_token}");

            client.DownloadData(request).SaveAs(downloadTo);
        }
    }
}

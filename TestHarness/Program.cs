using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ForgeFun.Core.Services;
using Serilog;

namespace ForgeFun.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new LoggerConfiguration().WriteTo.LiterateConsole().CreateLogger();

            var clientId = "";
            var clientSecret = "";
            var testFilePath = @"";
            var outputWebsite = @"";

            var scopes = new[] {"data:read", "data:write", "bucket:create", "data:search", "bucket:read" };

            var bucketKey = "forgefun";

            var authService = new AuthorizationService();
            var objectStoreService = new ObjectStoreService();
            var derivativeService = new DerivativeService();


            var testfileName = Path.GetFileName(testFilePath);

            log.Information("Get an authorization Token");
            var authToken = authService.GenerateToken(clientId, clientSecret, scopes);

            log.Information("Set the tokens for our services");
            objectStoreService.SetToken(authToken);
            derivativeService.SetToken(authToken);


            log.Information("List existing buckets for this application");
            var buckets = objectStoreService.ListBuckets();
            if (!buckets.Contains(bucketKey))
            {
                log.Information("If the bucket dosn't exist create it");
                objectStoreService.CreateBucket(bucketKey);
            }

            log.Information("List the bucket contents");
            var objects = objectStoreService.ListBucketContents(bucketKey);
            if (!objects.Contains(testfileName))
            {
                log.Information("If the bucket dosn't already contain our file upload it");
                objectStoreService.UploadFile(bucketKey, testfileName, testFilePath);
            }

            log.Information("Get our the objectId for our file");
            var testFileObjectId = objectStoreService.GetFileObjectId(bucketKey, testfileName);

            bool processing = false;
            log.Information("Check if our files has Derivatives");
            var exisitingDerivatives = derivativeService.GetDerivatives(testFileObjectId);
            if (exisitingDerivatives == null || exisitingDerivatives.Any(d => d.OutputType == "svf"))
            {
                var views = new[] { "2d", "3d" };
                log.Information("Issues the Convert to SVF");
                derivativeService.ConvertToSvf(testFileObjectId, views);
                processing = true;
            }

            while (processing)
            {
                log.Information("Waiting 10 seconds");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                var derivativesPercentageComplete = derivativeService.DerivativesPercentComplete(testFileObjectId);

                switch (derivativesPercentageComplete)
                {
                    case -1:
                        throw new Exception("Unexpected Processing State");
                    case 100:
                        processing = false;
                        break;
                    default:
                        log.Information("{derivativesPercentageComplete}% complete", derivativesPercentageComplete);
                        break;
                }

            } 
            
            var base64Urn = DerivativeService.Base64EncodeObjectId(testFileObjectId);
            var viewerAuthToken = authService.GenerateToken(clientId, clientSecret, new [] {"data:read"});

            var webSiteBuilder = new WebsiteBuilder();
            log.Information("Build the website");
            webSiteBuilder.Build(base64Urn, viewerAuthToken, outputWebsite);
            
        }
    }
}

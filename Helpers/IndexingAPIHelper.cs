using Google.Apis.Auth.OAuth2;
using Google.Apis.Indexing.v3;
using Google.Apis.Indexing.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace Sitecore.GoogleIndexingAPI.Helpers
{
    public static class IndexingAPIHelper
    {
        public static IndexingService _googleIndexingApiClientService = GetGoogleIndexingAPIClientService();
        public static IndexingService GetGoogleIndexingAPIClientService()
        {
            //Ensure to place the Private Key downloaded for the Google Service Account in the website root folder
            //JSON filename shall be updated for security reasons
            var privateKeyFilePath = HostingEnvironment.MapPath("/private-key.json");
            GoogleCredential credential;
            using (var stream = new FileStream(privateKeyFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(new[] { "https://www.googleapis.com/auth/indexing" });
            }
            return new IndexingService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });
        }

        public static void SendIndexingRequest(string linkToIndex, string action)
        {
            try
            {
                var publishRequestBody = new UrlNotification { Url = linkToIndex, Type = action };
                var publishIndexRequest = new UrlNotificationsResource.PublishRequest(_googleIndexingApiClientService, publishRequestBody);

                publishIndexRequest.ExecuteAsync().ConfigureAwait(false);
                Log.Info($"{action} request to Google Indexing API for '{linkToIndex}' succeeded", linkToIndex);
            }
            catch (Exception ex)
            {
                Log.Error($"{action} request to Google Indexing API for '{linkToIndex}' failed: {ex.Message}", ex, linkToIndex);
            }
        }

        public static void SendIndexingRequest(List<string> linksToIndex, string action)
        {
            try
            {
                var indexRequest = new BatchRequest(_googleIndexingApiClientService);
                var notificationResponses = new List<PublishUrlNotificationResponse>();
                foreach (var url in linksToIndex)
                {
                    var urlNotification = new UrlNotification { Url = url, Type = action };
                    indexRequest.Queue<PublishUrlNotificationResponse>(
                        new UrlNotificationsResource.PublishRequest(_googleIndexingApiClientService, urlNotification), (response, error, i, message) =>
                        {
                            notificationResponses.Add(response);
                        });
                }

                indexRequest.ExecuteAsync().ConfigureAwait(false);
                Log.Info($"{action} request to Google Indexing API for '{string.Join(",", linksToIndex)}' succeeded", linksToIndex);
            }
            catch (Exception ex)
            {
                Log.Error($"{action} request to Google Indexing API for '{string.Join(",", linksToIndex)}' failed: {ex.Message}", ex, linksToIndex);
            }
        }

        public static UrlNotificationMetadata VerifySubmissionStatus(string url)
        {
            var metadataRequest = new UrlNotificationsResource.GetMetadataRequest(_googleIndexingApiClientService) { Url = url };
            return metadataRequest.Execute();
        }

        public static List<UrlNotificationMetadata> VerifySubmissionStatus(List<string> urls)
        {
            var metadataRequest = new BatchRequest(_googleIndexingApiClientService);
            var metadataResponses = new List<UrlNotificationMetadata>();
            foreach (var url in urls)
            {
                metadataRequest.Queue<UrlNotificationMetadata>(
                    new UrlNotificationsResource.GetMetadataRequest(_googleIndexingApiClientService) { Url = url }, (response, error, i, message) =>
                    {
                        metadataResponses.Add(response);
                    });
            }
            metadataRequest.ExecuteAsync().ConfigureAwait(false);
            return metadataResponses;
        }
    }
}
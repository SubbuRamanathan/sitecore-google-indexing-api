using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using Sitecore.Publishing.Pipelines.Publish;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.GoogleIndexingAPI.Pipelines
{
    using Extensions;
    using Helpers;

    class IndexPublishedContentProcessor : PublishProcessor
    {
        private LinkProvider _linkProvider;
        public IndexPublishedContentProcessor(ProviderHelper<LinkProvider, LinkProviderCollection> providerHelper)
        {
            _linkProvider = providerHelper.Provider;
        }

        public override void Process(PublishContext context)
        {
            Assert.ArgumentNotNull(context, "context");

            if (context.Aborted)
                return;

            if (context.Statistics.Created > 0 || context.Statistics.Updated > 0)
            {
                List<ID> modifiedItemIDs = context.ProcessedPublishingCandidates.Keys.Select(i => i.ItemId).ToList();
                List<string> modifiedLinks = FetchModifiedLinks(context, modifiedItemIDs);
                IndexingAPIHelper.SendIndexingRequest(modifiedLinks, "URL_UPDATED");
            }
        }

        private List<string> FetchModifiedLinks(PublishContext context, List<ID> processedItemIDs)
        {
            var modifiedLinks = new List<string>();
            var urlOptions = new ItemUrlBuilderOptions() { AlwaysIncludeServerUrl = true };
            foreach (var itemID in processedItemIDs)
            {
                //Add condition(s) to include only Templates that hold short-lived content. Eg: Events, Job Posting
                //Ensure that the respective pages follow Google Structured Data Standards (https://developers.google.com/search/docs/data-types/video#broadcast-event , https://developers.google.com/search/docs/data-types/job-posting)
                Item processedItem = context.PublishOptions.TargetDatabase.GetItem(itemID);
                string processedItemLink = processedItem.GetUrl(_linkProvider, urlOptions);
                if (!string.IsNullOrEmpty(processedItemLink))
                {
                    modifiedLinks.Add(processedItemLink);
                }
            }
            return modifiedLinks;
        }
    }
}

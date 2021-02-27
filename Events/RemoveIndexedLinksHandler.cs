using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using System;

namespace Sitecore.GoogleIndexingAPI.Events
{
    using Extensions;
    using Helpers;

    public class RemoveIndexedLinksHandler
    {
        private LinkProvider _linkProvider;
        public RemoveIndexedLinksHandler(ProviderHelper<LinkProvider, LinkProviderCollection> providerHelper)
        {
            _linkProvider = providerHelper.Provider;
        }

        public void OnItemDeleting(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");

            Item item = Event.ExtractParameter(args, 0) as Item;

            if (item == null)
                return;

            if (item.Database.Name == "master")
                return;

            //Add condition(s) to include only Templates that hold short-lived content. Eg: Events, Job Posting
            //Ensure that the respective pages follow Google Structured Data Standards (https://developers.google.com/search/docs/data-types/video#broadcast-event , https://developers.google.com/search/docs/data-types/job-posting)
            var urlOptions = new ItemUrlBuilderOptions() { AlwaysIncludeServerUrl = true };
            var deletedLink = item.GetUrl(_linkProvider, urlOptions);
            if(!string.IsNullOrEmpty(deletedLink))
                IndexingAPIHelper.SendIndexingRequest(deletedLink, "URL_DELETED");
        }
    }
}
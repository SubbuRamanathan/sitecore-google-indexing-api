using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using Sitecore.Sites;
using System;
using System.Linq;

namespace Sitecore.GoogleIndexingAPI.Extensions
{
    public static partial class ItemExtensions
    {
        public static string GetUrl(this Item item, LinkProvider linkProvider, ItemUrlBuilderOptions urlBuilderOptions)
        {
            if (item.HasPresentationDetails())
            {
                //Ensure that the targetHostName attribute of Site Definition is set to host name of Content Delivery instance
                using (new SiteContextSwitcher(item.GetSiteContext()))
                {
                    return linkProvider.GetItemUrl(item, urlBuilderOptions);
                }
            }
            return string.Empty;
        }

        public static SiteContext GetSiteContext(this Item item)
        {
            string itemPath = item.Paths.FullPath;
            var site = SiteContextFactory.Sites
                .Where(s => !string.IsNullOrEmpty(s.RootPath) && itemPath.StartsWith(s.RootPath, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.RootPath.Length)
                .FirstOrDefault();
            if (site != null)
            {
                SiteContext siteContext = Factory.GetSite(site.Name);
                return siteContext;
            }
            return null;
        }

        public static bool HasPresentationDetails(this Item item)
        {
            if (item != null)
            {
                return item.Fields[FieldIDs.LayoutField] != null
                        && !string.IsNullOrEmpty(item.Fields[FieldIDs.LayoutField].Value);
            }
            return false;
        }
    }
}
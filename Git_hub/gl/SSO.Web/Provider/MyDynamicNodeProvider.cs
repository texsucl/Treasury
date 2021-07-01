using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SSO.WebProvider
{
    public class MyDynamicNodeProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode nodes)
        {
            var repository = new Repository();
            var userId = nodes.SiteMap.CacheKey;

            foreach (var item in repository.GetItems())
            {
                var node = new DynamicNode(item.Slug, item.DisplayName);
                node.RouteValues.Add("id", item.Slug);
                node.RouteValues.Add("action", "index");
                //node.RouteValues.Add("url", "index");
                //node.RouteValues.Add("route", "index");
                //System.Diagnostics.Debug.WriteLine(item.Slug);

                yield return node;
            }
        }
    }

    public class Repository
    {
        public IEnumerable<Something> GetItems()
        {
            yield return new Something { Slug = "Slug1", DisplayName = "DisplayName1" };
            yield return new Something { Slug = "Slug2", DisplayName = "DisplayName2" };

        }
    }

    public class Something
    {
        public string Slug { get; set; }
        public string DisplayName { get; set; }
    }
}
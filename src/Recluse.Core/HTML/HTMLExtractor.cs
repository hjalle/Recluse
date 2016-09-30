using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Recluse.Core.Document;

namespace Recluse.Core.HTML
{
    public static class HtmlExtractor
    {
        public class HtmlStructured
        {
            public List<WebLink> Outlinks { get; set; }
            public string Html { get; set; }
            public string CleanContent { get; set; }

        }

        private static Uri ToAbsoluteUri(this string href, Uri siteUrl)
        {
            var uri = new Uri(href, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
                return uri;

            return new Uri(siteUrl, href.Trim('/'));
        }
        public static List<WebLink> GetOutlinks(HtmlDocument doc, Uri baseUri)
        {
            var links = new List<WebLink>();
            var nodes = doc.DocumentNode.Descendants("a").Where(a => a.GetAttributeValue("href", null) != null);
            foreach (var linkNode in nodes)
            {
                var href = linkNode.GetAttributeValue("href", "#");
                if (!Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
                    continue;
                var url = href.ToAbsoluteUri(baseUri);
                var webLink = new WebLink();
                if(Uri.Compare(url, baseUri, UriComponents.Host, UriFormat.SafeUnescaped, StringComparison.CurrentCulture) == 0)
                {
                    webLink.LinkType = WebLinkType.Internal;
                } else
                {
                    webLink.LinkType = WebLinkType.External;
                }
                webLink.Scheme = url.Scheme;
                webLink.LinkText = linkNode.InnerText;
                webLink.Uri = url;
                links.Add(webLink);
            }
            return links;
        }

        public static string ConvertToHtml(byte[] array, Encoding srcEncoding, Encoding dstEncoding)
        {
            var encodedRight = Encoding.Convert(srcEncoding, dstEncoding, array);
            return Encoding.UTF8.GetString(encodedRight);
        }

        public static HtmlDocument GetHtmlDocument(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
        
    }
}

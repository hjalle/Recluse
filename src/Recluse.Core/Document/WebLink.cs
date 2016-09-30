using System;

namespace Recluse.Core.Document
{
    public class WebLink
    {
        public string LinkText { get; set; }
        public WebLinkType LinkType { get; set; }
        public string Scheme { get; set; }
        public Uri Uri { get; set; }
    }
}

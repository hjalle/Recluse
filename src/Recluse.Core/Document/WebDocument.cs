using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Recluse.Core.HTML;

namespace Recluse.Core.Document
{
    public class WebDocument
    {
        private string _contentType;
        private byte[] _data;
        private Uri _uri;
        private WebHeaderCollection _headers;
        private HttpStatusCode? _statusCode;
        public WebDocument(byte[] data, WebHeaderCollection headers, Uri uri, HttpStatusCode? statusCode, string contentType)
        {
            _uri = uri;
            _headers = headers;
            _data = data;
            _statusCode = statusCode;
            _contentType = contentType;
        }
        private string _html;

        public string Html
        {
            get
            {
                if (_html == null)
                {
                    _html = HtmlExtractor.ConvertToHtml(Data, Encoding.UTF8, Encoding.UTF8);
                }
                return _html;
            }

        }
        public List<WebLink> Links
        {
            get
            {
                var htmlDoc = HtmlExtractor.GetHtmlDocument(Html);
                return HtmlExtractor.GetOutlinks(htmlDoc, Uri);
            }
        }
        public byte[] Data
        {
            get
            {
                return _data;
            }
        }
        public WebHeaderCollection Headers
        {
            get
            {
                return _headers;
            }
        }
        public Uri Uri
        {
            get
            {
                return _uri;
            }
        }
        public HttpStatusCode? StatusCode
        {
            get
            {
                return _statusCode;
            }
        }
        public string ContentType
        {
            get
            {
                return _contentType;
            }
        }

    }
}

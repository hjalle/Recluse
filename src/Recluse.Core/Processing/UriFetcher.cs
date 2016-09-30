using System.IO;
using System.Net;
using System.Threading.Tasks;
using Recluse.Core.Document;

namespace Recluse.Core.Processing
{
    public class UriFetcher
    {
        public bool Completed { get; private set; }
        private ICrawlTask Task { get; set; }
        public CompletedTask CompletedTask { get; private set; } 
        public UriFetcher(ICrawlTask crawlTask)
        {
            Task = crawlTask;
            Completed = false;
        }

        public async Task<UriFetcher> Fetch()
        {
            HttpStatusCode? code = null;
            byte[] buffer = new byte[0];
            WebResponse response = null;

            var webRequest = WebRequest.Create(Task.Uri);

            try
            {
                response = await webRequest.GetResponseAsync();
                var stream = response.GetResponseStream();
                buffer = await ReadFullyAsync(stream);
                stream.Dispose();
                if (response is HttpWebResponse)
                {
                    code = (response as HttpWebResponse).StatusCode;
                }
            }
            catch (WebException we)
            {
                if (we.Response is HttpWebResponse)
                {
                    var exresponse = we.Response as HttpWebResponse;
                    code = exresponse.StatusCode;
                    response = exresponse;
                }
            }
            finally
            {
                Completed = true;
                CompletedTask = new CompletedTask();
                CompletedTask.FetchedDocument = new WebDocument(data: buffer, headers: response?.Headers, uri: Task.Uri, statusCode: code, contentType: response?.ContentType);
                CompletedTask.Task = Task;
            }

            return this;
        }
        private async static Task<byte[]> ReadFullyAsync(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

    }
}

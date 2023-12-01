using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradianceHW
{
    class AsyncWebBrowser
    {
        public WebBrowser WebBrowser { get; private set; }

        public string WaitFor { get; set; }

        private Semaphore waitFor;

        public AsyncWebBrowser(WebBrowser webBrowser)
        {
            this.WebBrowser = webBrowser;
            this.waitFor = new Semaphore(0, 1);
            this.WebBrowser.DocumentCompleted += DocumentLoaded;
        }

        public async Task<HtmlDocument> AsyncNavigate(string uri)
        {
            this.WaitFor = uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? uri : $"https://{uri}";

            Console.WriteLine($"Navigating to: {this.WaitFor}");
            this.WebBrowser.Navigate(this.WaitFor);
            return await this.GetCurrentPage().ConfigureAwait(true);
        }

        public async Task<HtmlDocument> GetPageWhenLoad(string uri)
        {
            this.WaitFor = uri;
            Console.WriteLine($"Waiting for page: {uri}");
            return await Task.Run(() =>
            {
                this.waitFor.WaitOne();
                Console.WriteLine($"Page loaded: {uri}");
                return this.WebBrowser.GetCurrentDocument();
            }).ConfigureAwait(true);
        }

        public async Task<HtmlDocument> GetCurrentPage()
        {
            return await Task.Run(() =>
            {
                this.waitFor.WaitOne();
                Console.WriteLine($"Current page obtained");
                return this.WebBrowser.GetCurrentDocument();
            }).ConfigureAwait(true);
        }

        private void DocumentLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                var loadedUrl = e.Url.AbsoluteUri;
                Console.WriteLine($"Document loaded: {loadedUrl}");

                if (Uri.Compare(e.Url, new Uri(this.WaitFor), UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Console.WriteLine("URL match: Exact match");
                }
                else
                {
                    Console.WriteLine($"Document loaded, but URL does not match: {loadedUrl}");
                }

                this.waitFor.Release();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DocumentLoaded: {ex.Message}");
            }
        }

        public void Dispose()
        {
            this.WebBrowser.DocumentCompleted -= this.DocumentLoaded;
        }
    }
}
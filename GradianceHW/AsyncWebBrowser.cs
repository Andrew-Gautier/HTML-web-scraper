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
    // Attempt to create a Uri from the input string
    if (!Uri.TryCreate(uri, UriKind.Absolute, out var normalizedUri))
    {
        // Handle invalid URL, log an error, or throw an exception
        Console.WriteLine($"Invalid URL: {uri}");
        return null;
    }

    // Use the normalized and validated URL for navigation
    this.WaitFor = normalizedUri.AbsoluteUri;
    Console.WriteLine($"Navigating to: {this.WaitFor}");
    this.WebBrowser.Navigate(this.WaitFor);

    // Return the result of GetCurrentPage
    return await this.GetCurrentPage().ConfigureAwait(true);
}
        // public async Task<HtmlDocument> AsyncNavigate(string uri)
        // {
            
        //     this.WaitFor = uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? uri : $"https://{uri}";

        //     Console.WriteLine($"Navigating to: {this.WaitFor}");
        //     this.WebBrowser.Navigate(this.WaitFor);
        //     return await this.GetCurrentPage().ConfigureAwait(true);
        // }

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

        // private void DocumentLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        // {
        //     try
        //     {
        //         var loadedUrl = e.Url.AbsoluteUri;
        //         Console.WriteLine($"Document loaded: {loadedUrl}");

        //         if (Uri.Compare(e.Url, new Uri(this.WaitFor), UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0)
        //         {
        //             Console.WriteLine("URL match: Exact match");
        //         }
        //         else
        //         {
        //             Console.WriteLine($"Document loaded, but URL does not match: {loadedUrl}");
        //         }

        //         this.waitFor.Release();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Exception in DocumentLoaded: {ex.Message}");
        //     }
        // }
        private void DocumentLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                var loadedUrl = e.Url;
                Console.WriteLine($"Document loaded: {loadedUrl}");

                var expectedUrl = new Uri(this.WaitFor);

                if (UrlsMatch(loadedUrl, expectedUrl))
                {
                    Console.WriteLine("URL match: Exact match");
                }
                else
                {
                    Console.WriteLine($"Document loaded, but URL does not match: {loadedUrl}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DocumentLoaded: {ex.Message}");
            }
            finally
            {
                this.waitFor.Release();
            }
        }

        private bool UrlsMatch(Uri url1, Uri url2)
        {
            return url1.Host == url2.Host && url1.AbsolutePath == url2.AbsolutePath && url1.Query == url2.Query;
        }
        public void Dispose()
        {
            this.WebBrowser.DocumentCompleted -= this.DocumentLoaded;
        }
    }
}
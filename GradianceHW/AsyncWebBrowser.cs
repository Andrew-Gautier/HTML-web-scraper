using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.WebBrowser.DocumentCompleted += this.DocumentLoaded;
        }

        public async Task<HtmlDocument> AsyncNavigate(string uri)
        {
            this.WaitFor = uri;
            this.WebBrowser.Navigate(uri);
            return await this.GetCurrentPage();
        }

        public async Task<HtmlDocument> GetPageWhenLoad(string uri)
        {
            this.WaitFor = uri;
            return await Task.Run((Func<HtmlDocument>)(() =>
            {
                this.waitFor.WaitOne();
                return this.WebBrowser.GetCurrentDocument();
            }));
        }

        public async Task<HtmlDocument> GetCurrentPage()
        {
            return await Task.Run((Func<HtmlDocument>)(() =>
            {
                this.waitFor.WaitOne();
                return this.WebBrowser.GetCurrentDocument();
            }));
        }

        private void DocumentLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsoluteUri == this.WaitFor)
            {
                this.waitFor.Release();
            }
        }

        public void Dispose()
        {
            this.WebBrowser.DocumentCompleted -= this.DocumentLoaded;
        }
    }
}

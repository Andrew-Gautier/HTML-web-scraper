using System;
using System.Windows.Forms;

namespace GradianceHW
{
    static class ExtensionMethods
    {
        // Define a delegate to get the HTML document
        delegate HtmlDocument GetHtmlDoc();

        // Extension method to safely get the current document from a WebBrowser control
        public static HtmlDocument GetCurrentDocument(this WebBrowser webBrowser)
        {
            // Check if the call is from a different thread
            if (webBrowser.InvokeRequired)
            {
                // If it is, invoke the method on the UI thread
                return webBrowser.Invoke(new GetHtmlDoc(webBrowser.GetCurrentDocument)) as HtmlDocument;
            }
            else
            {
                // If it's on the UI thread, directly return the document
                return webBrowser.Document;
            }
        }
    }
}

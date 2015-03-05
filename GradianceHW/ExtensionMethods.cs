using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradianceHW
{
    static class ExtensionMethods
    {
        delegate HtmlDocument GetHtmlDoc();
        public static HtmlDocument GetCurrentDocument(this WebBrowser webBrowser)
        {
            if (webBrowser.InvokeRequired)
            {
                return webBrowser.Invoke(new GetHtmlDoc(webBrowser.GetCurrentDocument)) as HtmlDocument;
            }
            else
            {
                return webBrowser.Document;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Collections.Specialized;

namespace GradianceHW
{
    class GradianceClass
    {
        private const string c_DirectHWNav = "Command=StudentHomeworks&Screen=HomePage:StudentHomeworks";
        public string Name { get; private set; }
        protected Uri Uri { get; private set; }
        protected AsyncWebBrowser WebBrowser { get; private set; }
        protected Uri DirectHomework { get; private set; }

        public GradianceClass(string name, string uri, AsyncWebBrowser webBrowser)
        {
            this.Name = name;
            this.Uri = new Uri(uri);
            var query = HttpUtility.ParseQueryString(this.Uri.Query);
            var s = "http://" + this.Uri.Host + this.Uri.AbsolutePath + "?" + c_DirectHWNav + "&sessionId=" + query["reqId"];
            this.DirectHomework = new Uri(s);
            this.WebBrowser = webBrowser;
        }

        public async Task<IEnumerable<GradianceHomework>> GetHomeworks()
        {
            var classPage = await this.WebBrowser.AsyncNavigate(this.Uri.AbsoluteUri);
            var homeworkPage = await this.WebBrowser.AsyncNavigate(this.DirectHomework.AbsoluteUri);

            IList<GradianceHomework> homeworks = new List<GradianceHomework>();

            var tableElements = homeworkPage.GetElementsByTagName("tr");

            IList<HtmlElement> tables = new List<HtmlElement>();

            foreach (HtmlElement table in tableElements)
            {
                if (table.GetAttribute("valign") == "top" && table.GetAttribute("className") == "smallfont")
                {
                    tables.Add(table);
                }
            }

            foreach (var table in tables)
            {
                var linkElements = table.GetElementsByTagName("a");

                var tdElements = table.GetElementsByTagName("td");

                HtmlElement nameElement = null;

                foreach (HtmlElement td in tdElements)
                {
                    if (td.GetAttribute("width") == "35%")
                    {
                        nameElement = td;
                        break;
                    }
                }

                string openHomeworkHref = null;

                foreach (HtmlElement link in linkElements)
                {
                    var href = link.GetAttribute("href");
                    if (href.Contains("Command=OpenHomework&"))
                    {
                        openHomeworkHref = href;
                        break;
                    }
                }

                var homework = new GradianceHomework(nameElement.InnerText, this.WebBrowser, openHomeworkHref);

                homeworks.Add(homework);
            }

            return homeworks;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace GradianceHW
{
    class GradianceClass
    {
        private const string DirectHWNavCommand = "Command=StudentHomeworks&Screen=HomePage:StudentHomeworks";

        public string Name { get; private set; }
        protected Uri Uri { get; private set; }
        protected AsyncWebBrowser WebBrowser { get; private set; }
        protected Uri DirectHomework { get; private set; }

        public GradianceClass(string name, string uri, AsyncWebBrowser webBrowser)
        {
            this.Name = name;
            this.Uri = new Uri(uri);
            var query = HttpUtility.ParseQueryString(this.Uri.Query);

            // Construct the direct homework URL
            var directHomeworkUri = new Uri($"http://{this.Uri.Host}{this.Uri.AbsolutePath}?{DirectHWNavCommand}&sessionId={query["reqId"]}");
            this.DirectHomework = directHomeworkUri;
            this.WebBrowser = webBrowser;
        }

        public async Task<IEnumerable<GradianceHomework>> GetHomeworks()
        {
            try{
            // Navigate to the class page
            Console.WriteLine($"Navigating to class page: {this.Uri.AbsoluteUri}");
            var classPage = await this.WebBrowser.AsyncNavigate(this.Uri.AbsoluteUri);

            // Navigate to the direct homework URL
             Console.WriteLine($"Navigating to homework page: {this.DirectHomework.AbsoluteUri}");
            var homeworkPage = await this.WebBrowser.AsyncNavigate(this.DirectHomework.AbsoluteUri);

            IList<GradianceHomework> homeworks = new List<GradianceHomework>();

            // Get all table elements
            var tableElements = homeworkPage.GetElementsByTagName("tr");

            IList<HtmlElement> tables = tableElements
                .Cast<HtmlElement>()
                .Where(table => table.GetAttribute("valign") == "top" && table.GetAttribute("className") == "smallfont")
                .ToList();

            foreach (var table in tables)
            {
                // Process table rows and extract homework information
                var homework = ProcessHomeworkTableRow(table);
                homeworks.Add(homework);
            }

            return homeworks;
            }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while getting homeworks for {this.Name}: {ex.Message}");
            throw;
        }
            
        }

        private GradianceHomework ProcessHomeworkTableRow(HtmlElement table)
        {
            var linkElements = table.GetElementsByTagName("a");
            var tdElements = table.GetElementsByTagName("td");

            HtmlElement nameElement = tdElements
                .Cast<HtmlElement>()
                .FirstOrDefault(td => td.GetAttribute("width") == "35%");

            string openHomeworkHref = linkElements
                .Cast<HtmlElement>()
                .FirstOrDefault(link => link.GetAttribute("href").Contains("Command=OpenHomework&"))
                ?.GetAttribute("href");

            return new GradianceHomework(nameElement?.InnerText, this.WebBrowser, openHomeworkHref);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradianceHW
{
    class GradianceContext : IDisposable
    {
        private const string c_HW_URL = "http://www.newgradiance.com/services/servlet/COTC";
        
        protected AsyncWebBrowser WebBrowser { get; private set; }

        public GradianceContext(WebBrowser webBrowser)
        {
            this.WebBrowser = new AsyncWebBrowser(webBrowser);
        }

        public async Task<IEnumerable<GradianceClass>> Login(string username, string password)
        {
            var loginPage = await this.WebBrowser.AsyncNavigate(c_HW_URL);

            var inputElements = loginPage.GetElementsByTagName("input");
            var formElements = loginPage.GetElementsByTagName("form");

            HtmlElement usernameTextbox = null;
            HtmlElement passwordTextBox = null;
            foreach (HtmlElement e in inputElements)
            {
                switch(e.GetAttribute("name"))
                {
                    case "userId":
                        usernameTextbox = e;
                        break;
                    case "password":
                        passwordTextBox = e;
                        break;
                }
            }

            HtmlElement loginForm = null;
            foreach (HtmlElement e in formElements)
            {
                if (e.GetAttribute("name") == "loginForm")
                {
                    loginForm = e;
                }
            }

            usernameTextbox.SetAttribute("value", username);
            passwordTextBox.SetAttribute("value", password);
            loginForm.InvokeMember("submit");

            loginPage = await this.WebBrowser.GetCurrentPage();

            inputElements = loginPage.GetElementsByTagName("input");

            var usernameTextBoxCount = inputElements.GetElementsByName("userID").Count;

            if (usernameTextBoxCount > 0)
            {
                return null;
            }

            var classElements = loginPage.GetElementsByTagName("a");

            IList<GradianceClass> classes = new List<GradianceClass>();

            foreach (HtmlElement e in classElements)
            {
                var href = e.GetAttribute("href");
                var value = e.InnerText;

                if (href != null && href != string.Empty && href != " " && value != string.Empty && value != " "
                    && value != "Log Out "
                    && value != "Self-study Class Tokens"
                    && value != "Update Account "
                    && value != "Update Password "
                    && value != "Home Page "
                    && value != "Help ")
                {
                    classes.Add(new GradianceClass(value, href, this.WebBrowser));
                }
            }

            return classes;
        }

        public void Dispose()
        {
            
        }
    }
}

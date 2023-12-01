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
        private const string c_HW_URL = "https://www.newgradiance.com/services/servlet/COTC";
        
        protected AsyncWebBrowser WebBrowser { get; private set; }

        public GradianceContext(WebBrowser webBrowser)
        {
            this.WebBrowser = new AsyncWebBrowser(webBrowser);
        }

        public async Task<IEnumerable<GradianceClass>> Login(string username, string password)
        {
            var loginPage = await this.WebBrowser.AsyncNavigate(c_HW_URL);
            if (loginPage == null)
{
    Console.WriteLine("Failed to navigate to login page.");
    // Handle the error, maybe return or throw an exception
    return null;
}
            var inputElements = loginPage.GetElementsByTagName("input");
            var formElements = loginPage.GetElementsByTagName("form");
            // Check if inputElements or formElements are null or empty
// if (inputElements == null || formElements == null || !inputElements.Any() || !formElements.Any())
// {
//     Console.WriteLine("Input or form elements not found on the login page.");
//     // Handle the error, maybe return or throw an exception
//     return null;
// }

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
        if (usernameTextbox == null || passwordTextBox == null || loginForm == null)
{
    Console.WriteLine("Username, password, or login form not found on the login page.");
    // Handle the error, maybe return or throw an exception
    return null;
}
            usernameTextbox.SetAttribute("value", username);
            passwordTextBox.SetAttribute("value", password);
            // Replace loginForm.InvokeMember("submit") with this code
var submitButton = loginForm.GetElementsByTagName("input").Cast<HtmlElement>()
                           .FirstOrDefault(e => e.GetAttribute("type") == "image");
submitButton?.InvokeMember("click");

            loginPage = await this.WebBrowser.GetCurrentPage();
            
            inputElements = loginPage.GetElementsByTagName("input");

            var usernameTextBoxCount = inputElements.GetElementsByName("userId").Count;
            
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

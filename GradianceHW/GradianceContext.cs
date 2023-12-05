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

        public async Task<List<GradianceClass>> Login(string username, string password)
        {
            try
            {
                Console.WriteLine("Navigating to login page...");
                var loginPage = await this.WebBrowser.AsyncNavigate(c_HW_URL);
                if (loginPage == null)
                {
                    throw new Exception("Failed to navigate to login page.");
                }

                Console.WriteLine("Retrieving input and form elements...");
                var inputElements = loginPage.GetElementsByTagName("input");
                var formElements = loginPage.GetElementsByTagName("form");

                if (inputElements == null || formElements == null || inputElements.Count == 0 || formElements.Count == 0)
                {
                    throw new Exception("Input or form elements not found on the login page.");
                }

                var usernameTextbox = inputElements.Cast<HtmlElement>().FirstOrDefault(e => e.GetAttribute("name") == "userId");
                var passwordTextBox = inputElements.Cast<HtmlElement>().FirstOrDefault(e => e.GetAttribute("name") == "password");
                var loginForm = formElements.Cast<HtmlElement>().FirstOrDefault(e => e.GetAttribute("name") == "loginForm");

                if (usernameTextbox == null || passwordTextBox == null || loginForm == null)
                {
                    throw new Exception("Username, password, or login form not found on the login page.");
                }

                Console.WriteLine("Setting username and password...");
                usernameTextbox.SetAttribute("value", username);
                passwordTextBox.SetAttribute("value", password);

                Console.WriteLine("Submitting login form...");
                var submitButton = loginForm.GetElementsByTagName("input")
                                            .Cast<HtmlElement>()
                                            .FirstOrDefault(e => e.GetAttribute("type") == "image");

                if (submitButton == null)
                {
                    throw new Exception("Submit button not found on the login form.");
                }

                submitButton.InvokeMember("click");

                Console.WriteLine("Waiting for login to complete...");
                loginPage = await this.WebBrowser.GetCurrentPage();

                Console.WriteLine("Checking if login was successful...");
                var usernameTextBoxCount = loginPage.GetElementsByTagName("input")
                                                    .GetElementsByName("userId")
                                                    .Count;

                if (usernameTextBoxCount > 0)
                {
                    throw new Exception("Login failed. Invalid username or password.");
                }

                Console.WriteLine("Retrieving class elements...");
                var classElements = loginPage.GetElementsByTagName("a");

                List<GradianceClass> classes = new List<GradianceClass>();

                foreach (HtmlElement e in classElements)
                {
                    var href = e.GetAttribute("href");
                    var value = e.InnerText;

                    if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(value)
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
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during login: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            
        }
    }
}

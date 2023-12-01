using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradianceHW
{
    class GradianceUserSession
    {
        public string Username { get; private set; }
        protected WebBrowser WebBrowser { get; private set; }

        public GradianceUserSession(string username, WebBrowser webBrowser)
        {
            this.Username = username;
            this.WebBrowser = webBrowser;

            // Debugging statement to check if the constructor is called
            Console.WriteLine("GradianceUserSession constructor called");

            // Debugging statement to display the username
            Console.WriteLine("Username: " + this.Username);

            // Debugging statement to display the web browser instance
            Console.WriteLine("WebBrowser: " + this.WebBrowser);
        }
    }
}

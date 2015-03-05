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
        }
    }
}

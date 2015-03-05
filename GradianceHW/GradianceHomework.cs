using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace GradianceHW
{
    class GradianceHomework
    {
        public string Name { get; private set; }
        
        protected AsyncWebBrowser WebBrowser { get; private set; }
        protected Uri OpenHomeworkUri { get; private set; }
        protected Uri PastSubmissionsUri { get; private set; }

        private IDictionary<string, bool?> Questions { get; set; }

        private Timer timer { get; set; }

        private bool doable;

        public GradianceHomework(string name, AsyncWebBrowser webBrowser, string openHomeworkLink)
        {
            this.Name = name;
            this.WebBrowser = webBrowser;
            this.OpenHomeworkUri = new Uri(openHomeworkLink);
            var query = HttpUtility.ParseQueryString(this.OpenHomeworkUri.Query);
            this.PastSubmissionsUri = new Uri("http://" + this.OpenHomeworkUri.Host + this.OpenHomeworkUri.AbsolutePath + "?sessionId=" + query["sessionId"] + "&Command=ViewPastSubmissions&testId=" + query["testId"] + "&groupId=" + query["groupId"]);
            this.Questions = new Dictionary<string, bool?>();
            this.timer = new Timer();
            timer.Interval = 600000;
            timer.Tick += async (s, e) =>
            {
                doable = true;
                timer.Stop();
                if (MessageBox.Show(this.Name + " is ready to try again!", "Done waiting.", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                {
                    await this.LoadPastSubmissions();
                    await this.DoHomework();
                }
            };
            this.doable = true;
        }

        public async Task DoHomework()
        {
            if (!this.doable) return;

            var homeworkPage = await this.WebBrowser.AsyncNavigate(this.OpenHomeworkUri.AbsoluteUri);

            var inputElements = homeworkPage.GetElementsByTagName("input");

            IDictionary<string, IList<HtmlElement>> radioGroups = new Dictionary<string, IList<HtmlElement>>();

            HtmlElement submitButton = null;

            foreach (HtmlElement inputElement in inputElements)
            {
                if (inputElement.GetAttribute("type") == "radio")
                {
                    var name = inputElement.GetAttribute("name");

                    if (radioGroups.ContainsKey(name))
                    {
                        radioGroups[name].Add(inputElement);
                    }
                    else
                    {
                        radioGroups[name] = new List<HtmlElement>();
                        radioGroups[name].Add(inputElement);
                    }
                }
                else if (inputElement.GetAttribute("value") == "Submit Homework")
                {
                    submitButton = inputElement;
                }
            }

            IList<IDictionary<string, HtmlElement>> answerRadioButtonGroups = new List<IDictionary<string, HtmlElement>>();

            foreach (var kvp in radioGroups)
            {
                var answerRadioButton = new Dictionary<string, HtmlElement>();

                foreach (var e in kvp.Value)
                {
                    var letterElement = e.Parent.NextSibling;

                    var textElement = letterElement.NextSibling;

                    var text = textElement.InnerText.Trim().Replace("\n", "").Replace("\r", "");

                    answerRadioButton[text] = e;
                }

                answerRadioButtonGroups.Add(answerRadioButton);
            }

            foreach (var group in answerRadioButtonGroups)
            {
                var maybes = new List<HtmlElement>();

                HtmlElement correct = null;

                var unknowns = new List<HtmlElement>();

                foreach (var kvp in group)
                {
                    bool? result = null;
                    if (this.Questions.TryGetValue(kvp.Key, out result))
                    {
                        if (result == null)//could have been correct
                        {
                            maybes.Add(kvp.Value);
                        }
                        else if ((bool)result)//correct
                        {
                            correct = kvp.Value;
                        }
                    }
                    else
                    {
                        unknowns.Add(kvp.Value);
                    }
                }

                if (correct != null)
                {
                    correct.InvokeMember("keypress");
                    correct.InvokeMember("click");
                }
                else if (maybes.FirstOrDefault() != null)
                {
                    maybes.FirstOrDefault().InvokeMember("keypress");
                    maybes.FirstOrDefault().InvokeMember("click");
                }
                else if (unknowns.FirstOrDefault() != null)
                {
                    unknowns.FirstOrDefault().InvokeMember("keypress");
                    unknowns.FirstOrDefault().InvokeMember("click");
                }
            }

            if (submitButton != null)
            {
                doable = false;
                submitButton.InvokeMember("click");
                timer.Start();
            }
        }

        public async Task LoadPastSubmissions()
        {
            var pastSubsPage = await this.WebBrowser.AsyncNavigate(this.PastSubmissionsUri.AbsoluteUri);

            var linkElements = pastSubsPage.GetElementsByTagName("a");

            IList<string> pastSubmissionLinks = new List<string>();

            foreach (HtmlElement link in linkElements)
            {
                if (link.InnerText == "submission ")
                {
                    pastSubmissionLinks.Add(link.GetAttribute("href"));
                }
            }

            foreach (var pastSubmissionLink in pastSubmissionLinks)
            {
                var pastSubmissionPage = await this.WebBrowser.AsyncNavigate(pastSubmissionLink);

                var answersElements = pastSubmissionPage.GetElementsByTagName("table");

                foreach (HtmlElement answersElement in answersElements)
                {
                    if (answersElement.GetAttribute("border") == "0" 
                        && answersElement.GetAttribute("cellpadding") == "3"
                        && answersElement.GetAttribute("cellspacing") == "0"
                        && answersElement.GetAttribute("width") == "100%")
                    {
                        HtmlElement correctAnswerTableElement = answersElement.NextSibling;

                        var posAnswerElements = answersElement.GetElementsByTagName("td");

                        var answerElements = new Dictionary<char, string>();

                        foreach (HtmlElement posAnswerElement in posAnswerElements)
                        {
                            string txt = posAnswerElement.InnerText ?? string.Empty;

                            if (posAnswerElement.GetAttribute("width") == "4%" && txt.Contains(")"))
                            {
                                var answerLetter = txt[0];

                                var answerText = posAnswerElement.NextSibling.InnerText.Trim().Replace("\n", "").Replace("\r", "");

                                answerElements[answerLetter] = answerText;
                            }
                        }

                        var correctAnswerElements = correctAnswerTableElement.GetElementsByTagName("td");

                        char submmitedAnswer = ' ';
                        var isCorrect = false;

                        foreach (HtmlElement correctAnswer in correctAnswerElements)
                        {
                            if (correctAnswer.InnerText == null) continue;

                            var txt = correctAnswer.InnerText.Trim();

                            if (txt.Contains("Answer submitted:"))
                            {
                                var a = correctAnswer.GetElementsByTagName("b");

                                foreach (HtmlElement x in a)
                                {
                                    if (x.InnerText == null) continue;

                                    if (x.InnerText.Contains(')'))
                                    {
                                        submmitedAnswer = x.InnerText[0];
                                    }
                                }
                            }
                            else if (txt.Contains("correctly"))
                            {
                                isCorrect = true;
                            }
                        }

                        if (submmitedAnswer != ' ')
                        {
                            this.Questions[answerElements[submmitedAnswer]] = isCorrect;
                        }

                        foreach (var kvp in answerElements)
                        {
                            if (kvp.Key != submmitedAnswer)
                            {
                                this.Questions[answerElements[kvp.Key]] = isCorrect ? (bool?)false : null;
                            }
                        }
                    }
                }
            }



        }

        public override string ToString()
        {
            return this.Name;
        }

    }
}

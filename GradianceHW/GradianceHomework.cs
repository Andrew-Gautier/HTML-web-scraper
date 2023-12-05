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
            // Changed here. 11-30
            var uriBuilder = new UriBuilder(this.OpenHomeworkUri.Scheme, this.OpenHomeworkUri.Host, this.OpenHomeworkUri.Port, this.OpenHomeworkUri.AbsolutePath);
            uriBuilder.Query = $"sessionId={query["sessionId"]}&Command=ViewPastSubmissions&testId={query["testId"]}&groupId={query["groupId"]}";
            this.PastSubmissionsUri = uriBuilder.Uri;
            this.Questions = new Dictionary<string, bool?>();
            this.timer = new Timer();
            timer.Interval = 600000;
            timer.Tick += async (s, e) =>
            {

                doable = true;
                timer.Stop();
                if (MessageBox.Show(this.Name + " is ready to try again!", "Done waiting.", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                {
                    try
                    {
                        await this.LoadPastSubmissions();
                        await this.DoHomework();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while doing homework: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            this.doable = true;
        }

        public async Task DoHomework()
        {
            if (!this.doable) return;

            try
            {
                var homeworkPage = await this.WebBrowser.AsyncNavigate(this.OpenHomeworkUri.AbsoluteUri);
                Log("Navigated to homework page: " + this.OpenHomeworkUri.AbsoluteUri);

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
                        Console.WriteLine("Executing correct answer loop");
                        correct.InvokeMember("keypress");
                        correct.InvokeMember("click");
                    }
                    else if (maybes.FirstOrDefault() != null)
                    {
                        Console.WriteLine("Executing maybes loop");
                        maybes.FirstOrDefault().InvokeMember("keypress");
                        maybes.FirstOrDefault().InvokeMember("click");
                    }
                    else if (unknowns.FirstOrDefault() != null)
                    {
                        Console.WriteLine("Executing unknowns loop");
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
            catch (Exception ex)
            {
                throw new Exception("An error occurred while doing homework: " + ex.Message, ex);
            }
        }

        public async Task<bool> LoadPastSubmissions()
        {
            try
            {
                var pastSubsPage = await this.WebBrowser.AsyncNavigate(this.PastSubmissionsUri.AbsoluteUri);
                Log("Navigated to past submissions page:  111 " + this.PastSubmissionsUri.AbsoluteUri);

                var linkElements = pastSubsPage.GetElementsByTagName("a");

                IList<string> pastSubmissionLinks = new List<string>();

                foreach (HtmlElement link in linkElements)
                {
                    if (link.InnerText == "submission ")
                    {
                        pastSubmissionLinks.Add(link.GetAttribute("href"));
                    }
                    else
                    {
                        Log("Link InnerText: 444" + link.InnerText);
                        //Log("Submissions Page HTML: " + pastSubsPage.Body.OuterHtml);
                    }
                }

                if (pastSubmissionLinks.Count == 0)
                {
                    return false; // No past submissions found
                }

                Log("Here are my total number of pages" + pastSubmissionLinks.Count);

                foreach (var pastSubmissionLink in pastSubmissionLinks)
                {
                    var pastSubmissionPage = await this.WebBrowser.AsyncNavigate(pastSubmissionLink);
                    //Log("Navigated to past submission page:  222" + pastSubmissionLink);
                    //Log("Submissions Page HTML check&&&&: " + pastSubmissionPage.Body.OuterHtml);
                    var tableElements = pastSubmissionPage.GetElementsByTagName("table");

                    Log("Here are my table elements: " + tableElements.Count);

                    foreach (HtmlElement tableElement in tableElements)
                    {
                        if (tableElement.GetAttribute("border") == "0"
                            && tableElement.GetAttribute("cellpadding") == "3"
                            && tableElement.GetAttribute("cellspacing") == "0"
                            && tableElement.GetAttribute("width") == "100%")
                        {
                            HtmlElement correctAnswerTableElement = tableElement.NextSibling;

                            var tdElements = tableElement.GetElementsByTagName("td");

                            var answerElements = new Dictionary<char, string>();
                            var trElements = tableElement.GetElementsByTagName("tr");
                            char submmitedAnswer = ' ';
                            var isCorrect = false;

                            foreach (HtmlElement tdElement in tdElements)
                            {

                                if (tdElement.InnerText != null && tdElement.InnerText.Contains(")"))
                                {
                                    var answerLetter = tdElement.InnerText[0];
                                    string answerText = null;
                                    if (tdElement.NextSibling != null && tdElement.NextSibling.InnerText != null)
                                    {
                                        answerText = tdElement.NextSibling.InnerText.Trim().Replace("\n", "").Replace("\r", "");
                                    }
                                    if (answerText != null)
                                    {
                                        answerElements[answerLetter] = answerText;
                                    }
                                    //Log("Answer Letter: " + answerLetter);
                                    Log("Answer Text: " + answerText);
                                }
                                // ABOVE CODE WORKS 
                            }
                            

                            foreach (HtmlElement trElement in trElements)
                            {
                                Log("TR Element Inner Text: " + trElement.InnerText);
                                Log("TR Element Style: " + trElement.Style);
                                var innerText = trElement.InnerText?.Trim().ToLower();
                                if (innerText?.StartsWith("answer submitted:") == true)
                                {
                                    var submittedAnswerIndex = innerText.IndexOf(':') + 1;
                                    var submittedAnswer = innerText.Substring(submittedAnswerIndex).Trim();
                                    Log("TR Element Inner Text: " + trElement.InnerText);
                                    Log("Submitted Answer: " + submittedAnswer);
                                    if (!string.IsNullOrEmpty(submittedAnswer) && submittedAnswer.Length > 0)
                                    {
                                        submmitedAnswer = submittedAnswer[0];
                                    }
                                }
                                else if (innerText?.Contains("your answer is incorrect.") == true)
                                {
                                    isCorrect = false;
                                }
                            }

                            if (submmitedAnswer != ' ' && answerElements.ContainsKey(submmitedAnswer))
                            {
                                this.Questions[answerElements[submmitedAnswer]] = isCorrect;
                            }

                            foreach (var kvp in answerElements)
                            {
                                if (kvp.Key != submmitedAnswer && this.Questions.ContainsKey(kvp.Value))
                                {
                                    this.Questions[kvp.Value] = isCorrect ? (bool?)false : null;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while loading past submissions: " + ex.Message, ex);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}

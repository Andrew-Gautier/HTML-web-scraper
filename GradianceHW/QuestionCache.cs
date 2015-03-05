using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradianceHW
{
    class QuestionCache
    {
        public string Question { get; set; }
        public IDictionary<string, string> Correct { get; set; }
        public IDictionary<string, string> Incorrect { get; set; }
    }
}

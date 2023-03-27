using Blueprint.Blue;
using Pinshot.PEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintBlue.Model.Search
{
    public class QSearchFeature
    {
        private string Text;
        public List<ITerm> Terms { get; private set; }
        public QFind Context { get; private set; }

        public QSearchFeature(QFind context, string text, Parsed[] args)
        {
            this.Text = text;
            this.Terms = new();
            this.Context = context;

            foreach (var arg in args)
            {
                var term = QTerm.Create(arg.text, arg);
                this.Terms.Add(term);
            }
        }
    }
}

namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using AVXLib;
    using Pinshot.PEG;
    using System.Data;
    using static System.Net.Mime.MediaTypeNames;

    public class QFilter: SearchFilter
    {
        public string Filter { get; private set; }

        private QFilter(string filter): base()
        {
            this.Filter = filter;
        }
        public static QFilter? Create(IDiagnostic diagnostics, string text, Parsed[] args)
        {
            return args.Length == 1 ? new QFilter(args[0].text) : null;
        } 
    }
}
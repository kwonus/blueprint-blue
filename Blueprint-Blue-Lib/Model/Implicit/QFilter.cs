namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using AVXLib;
    using Pinshot.PEG;
    using System.Data;
    using static System.Net.Mime.MediaTypeNames;

    public class QFilter
    {
        public string Filter { get; private set; }

        private QFilter(string filter)
        {
            this.Filter = filter;
        }
        public static QFilter? Create(Parsed filter)
        {
            if (filter.rule.Equals("filter", StringComparison.InvariantCultureIgnoreCase))
            {
                if (filter.children.Length == 1)
                    return new QFilter(filter.children[0].text);
            }
            return null;
        } 
    }
}
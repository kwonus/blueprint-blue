namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    public class QFilter
    {
        public string Filter { get; private set; }

        private QFilter(IDiagnostic diagnostics, string filter)
        {
            this.Filter = filter;
        }
        public static QFilter? Create(IDiagnostic diagnostics, string text, Parsed[] args)
        {
            return args.Length == 1 ? new QFilter(diagnostics, args[0].text) : null;
        }
    }
}
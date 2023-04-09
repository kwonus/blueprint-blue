namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    public class QDisplay : QImplicitCommand, ICommand
    {
        public override string Expand()
        {
            return this.Text;
        }
        public List<uint> Fields { get; private set; }

        private QDisplay(QContext env, string text, IEnumerable<uint> fields) : base(env, text, "display")
        {
            this.Fields = new();

            foreach (var field in fields)
                this.Fields.Add(field);
        }
        public static QDisplay? Create(QContext env, string text, Parsed[] args)
        {
            try
            {
                var fields = from field in args where field.rule.Equals("DIGITS", StringComparison.InvariantCultureIgnoreCase) select uint.Parse(field.text);
                return new QDisplay(env, text, (fields));
            }
            catch
            {
                ;
            }
            return null;
        }
    }
}
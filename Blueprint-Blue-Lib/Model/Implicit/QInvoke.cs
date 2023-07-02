using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QInvoke : QImplicitCommand, ICommand
    {
        public string Label { get; set; } // macro
        public uint Command { get; set; } // history
        public bool Current { get; set; } // Is the ::current suffix present on the invokation?

        public override string Expand()
        {
            return this.Text;
        }
        private QInvoke(QContext env, string text, string label) : base(env, text, "invoke")
        {
            this.Command = 0;
            this.Label = label;
            this.Current = text.EndsWith("::current", StringComparison.InvariantCultureIgnoreCase);
        }
        private QInvoke(QContext env, string text, uint command) : base(env, text, "invoke")
        {
            this.Command = command;
            this.Label = string.Empty;
            this.Current = text.EndsWith("::current", StringComparison.InvariantCultureIgnoreCase);
        }
        // TODO: COnditionally add QCurrent 
        public static QInvoke? Create(QContext env, string text, Parsed[] args)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string trimmed = text.Trim();
            bool numerics = true;
            uint accumulator = 0;
            for (int i = 0; i < trimmed.Length; i++)
            {
                numerics = char.IsDigit(trimmed[i]);
                if (!numerics)
                    break;
                accumulator *= 10;
                accumulator += (uint)(trimmed[i] - '0');
            }
            if (numerics)
                return accumulator > 0 ? new QInvoke(env, trimmed, accumulator) : null;
            else
                return args.Length == 1 && args[0].rule.ToLower() == "label" ? new QInvoke(env, trimmed, args[0].text) : null;
        }
    }
}
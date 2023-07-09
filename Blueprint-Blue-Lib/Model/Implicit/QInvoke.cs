using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QInvoke : QImplicitCommand, ICommand
    {
        public string Generic { get; set; }
        public string Label { get; set; } // macro
        public uint Command { get; set; } // history
        public bool Deterministic { get; set; }

        public void MakeNonDeterministic()
        {
            this.Deterministic = false;
        }

        public override string Expand()
        {
            return this.Text;
        }
        private QInvoke(QContext env, string text, string label, bool useCurrentEnv) : base(env, text, "invoke")
        {
            this.Deterministic = !useCurrentEnv;
            this.Command = 0;
            this.Label = label;
            this.Generic = label;
        }
        private QInvoke(QContext env, string text, uint command, bool useCurrentEnv) : base(env, text, "invoke")
        {
            this.Deterministic = !useCurrentEnv;
            this.Command = command;
            this.Label = string.Empty;
            this.Generic = command.ToString();
        }

        public static QInvoke? Create(QContext env, string text, Parsed[] args)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string generic = args.Length >= 1 && args[0].rule.ToLower() == "invoke" && args[0].children.Length == 1 ? args[0].children[0].text : string.Empty;

            if (!string.IsNullOrWhiteSpace(generic))
            {
                bool numerics = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower() == "historic";
                bool labelled = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower() == "label";

                bool current = (numerics || labelled) && (args.Length == 2) && (args[1].rule == "control_suffix");

                if (numerics)
                {
                    uint accumulator = 0;
                    for (int i = 0; i < generic.Length; i++)
                    {
                        numerics = char.IsDigit(generic[i]);
                        if (!numerics)
                            return null;
                        accumulator *= 10;
                        accumulator += (uint)(generic[i] - '0');
                    }
                    return accumulator > 0 ? new QInvoke(env, text, accumulator, current) : null;
                }
                else if (labelled)
                {
                    return new QInvoke(env, text, generic, current);
                }
            }
            return null;
        }
    }
}
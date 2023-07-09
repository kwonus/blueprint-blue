using Pinshot.PEG;
using XBlueprintBlue;

namespace Blueprint.Blue
{
    public class QAbsorb : QExplicitCommand, ICommand
    {
        public string Label   { get; set; } // macro
        public uint   Command { get; set; } // history
        public string Generic { get; set; } // either
        public QAbsorb(QContext env, string text, Parsed[] args) : base(env, text, "absorb")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                this.Generic = string.Empty;
                this.Label = string.Empty;
                this.Command = 0;
                return;
            }
            this.Generic = args != null && args.Length == 1 ? args[0].text : string.Empty;
            bool numerics = true;
            uint accumulator = 0;
            for (int i = 0; i < this.Generic.Length; i++)
            {
                char c = this.Generic[i];
                numerics = char.IsDigit(c);
                if (!numerics)
                    break;
                accumulator *= 10;
                accumulator += (uint)(c - '0');
            }
            if (numerics)
            {
                this.Command = accumulator;
                this.Label = string.Empty;
            }
            else
            {
                this.Command = 0;
                this.Label = this.Generic;
            }
        }
        public override void AddArgs(XCommand command)
        {
            if (command != null && command.Arguments != null)
                command.Arguments.Add(this.Generic);
        }
    }
}
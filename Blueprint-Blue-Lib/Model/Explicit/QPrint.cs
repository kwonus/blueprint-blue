namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Text;
    using YamlDotNet.Serialization;
    using Blueprint.Blue;

    public class QPrint : QExplicitCommand, ICommand
    {
        public List<(byte b, byte c, byte v1, byte v2)> Verses { get; private set; }

        public QPrint(QContext env, string text, Parsed[] args) : base(env, text, "print")
        {
            Verses = new();

            //foreach (var field in fields)
                //Verses.Add(field);

            /*
            for (int additional = 1; additional < stmt.children.Length; additional++)
            {
                var clause = stmt.children[additional];
                if (clause.rule.Equals("export", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (implicits.ExportDirective == null) // these are implicit singletons; grammar should enforce this, but enforce it here too
                        implicits.ExportDirective = QExport.Create(context, clause.text, clause.children);
                }
                else if (clause.rule.Equals("print", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (implicits.LimitDirective == null) // these are implicit singletons; grammar should enforce this, but enforce it here too
                        implicits.LimitDirective = QLimit.Create(context, clause.text, clause.children);
                }
            }
            */
        }
        public override List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "not implemented yet.");
        }
    }
}
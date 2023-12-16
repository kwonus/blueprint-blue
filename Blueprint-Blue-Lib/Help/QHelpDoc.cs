namespace Blueprint.Blue
{
    using System.Collections.Generic;

    public class QHelpDoc
    {
        private QHelpDoc(string topic, string doc)
        {
            this.Topic = topic;
            this.Document = doc;
        }
        public string Topic { get; private set; }
        public string Document { get; private set; }

        private static Dictionary<string, QHelpDoc> Reference = new()
        {
            // Explicit Commands:
            { "help",       new(topic: "@help",       doc: "System"   ) },
            { "exit",       new(topic: "@exit",       doc: "System"   ) },
            { "delete",     new(topic: "@delete",     doc: "Macro"    ) },
            { "expand",     new(topic: "@expand",     doc: "Macro"    ) },
            { "review",     new(topic: "@review",     doc: "History"  ) },
            { "initialize", new(topic: "@initialize", doc: "History"  ) },
            { "get",        new(topic: "@get",        doc: "Control"  ) },
            { "set",        new(topic: "@set",        doc: "Control"  ) },
            { "clear",      new(topic: "@clear",      doc: "Control"  ) },
            { "absorb",     new(topic: "@absorb",     doc: "Control"  ) },
             
            // Implicit Commands:
            { "export",     new(topic: "export",      doc: "Output"   ) },
            { "limit",      new(topic: "limit",       doc: "Output"   ) },
            { "invoke",     new(topic: "invoke",      doc: "History"  ) },
            { "filter",     new(topic: "filter",      doc: "Search"   ) },
            { "find",       new(topic: "find",        doc: "Search"   ) },
            { "apply",      new(topic: "apply",       doc: "Macro"    ) },
            { "assign",     new(topic: "assign",      doc: "Control"  ) },

            // Topics:
            { "search",     new(topic: "#search",     doc: "Search"   ) },
            { "control",    new(topic: "#control",    doc: "Control"  ) },
            { "settings",   new(topic: "#settings",   doc: "Control"  ) }, // settings is an alias for control
            { "history",    new(topic: "#history",    doc: "History"  ) },
            { "macro",      new(topic: "#macro",      doc: "Macro"    ) },
            { "label",      new(topic: "#label",      doc: "Macro"    ) }, // label is an alias for macro
            { "output",     new(topic: "#output",     doc: "Output"   ) },
            { "system",     new(topic: "#system",     doc: "System"   ) }

        };

        public static QHelpDoc GetDocument(string verb)
        {
            if (verb != null)
            {
                string key = verb.Trim().ToLower();
                if (key.StartsWith('@') && (key.Length > 1))
                    key = key.Substring(1);

                if (QHelpDoc.Reference.ContainsKey(key))
                    return QHelpDoc.Reference[key];
            }
            return QHelpDoc.Reference["help"];
        }
    }
}

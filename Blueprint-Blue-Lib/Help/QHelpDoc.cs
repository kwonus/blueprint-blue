namespace Blueprint.Blue
{
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
            { "help",     new(topic: "@help",    doc: "Help"     ) },
            { "exit",     new(topic: "@exit",    doc: "Exit"     ) },
            { "version",  new(topic: "@version", doc: "Version"  ) },
            { "delete",   new(topic: "@delete",  doc: "Macro"    ) },
            { "expand",   new(topic: "@expand",  doc: "Macro"    ) },
            { "review",   new(topic: "@review",  doc: "History"  ) },
            { "get",      new(topic: "@get",     doc: "Settings" ) },
                                                         
            { "clear",    new(topic: "clear",    doc: "Settings" ) },
            { "display",  new(topic: "display",  doc: "Display"  ) },
            { "exec",     new(topic: "exec",     doc: "Exec"     ) },
            { "export",   new(topic: "export",   doc: "Export"   ) },
            { "filter",   new(topic: "filter",   doc: "Filter"   ) },
            { "find",     new(topic: "find",     doc: "Find"     ) },
            { "invoke",   new(topic: "invoke",   doc: "Macro"    ) },
            { "macro",    new(topic: "macro",    doc: "Macro"    ) },
            { "set",      new(topic: "set",      doc: "Settings" ) },
                                                         
            { "settings", new(topic: "settings", doc: "Settings" ) },
            { "history",  new(topic: "history",  doc: "History"  ) }
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

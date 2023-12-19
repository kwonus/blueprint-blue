namespace Blueprint.Blue
{
    using BlueprintBlue;
    using Pinshot.Blue;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class QStatement
    {
        public string Text { get; set; }
        public bool IsValid { get; set; }
        public string ParseDiagnostic { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public Dictionary<string, string> Disposition { get; set; }

        public QExplicitCommand? Singleton { get; set; }
        public QImplicitCommands? Commands { get; set; }
        public QContext Context { get; private set; }

        private QStatement()
        {
            this.Text = string.Empty;
            this.IsValid= false;
            this.ParseDiagnostic= string.Empty;
            this.Errors = new();
            this.Warnings = new();
            this.Disposition = new();
            this.Singleton = null;
            this.Commands = null;
            this.Context = new QContext(this);
        }

        public static QStatement Create(RootParse? root)
        {
            if (root != null)
            {
                QStatement stmt = new QStatement();
                stmt.IsValid = false;

                if (!string.IsNullOrEmpty(stmt.ParseDiagnostic))
                {
                    stmt.Errors.Add("See parse diagnostic for syntax errors.");
                    stmt.IsValid = false;
                }
                else if (root.result.Length == 1) // all parses in Quelle should result in a single statement
                {
                    var statement = root.result[0];
                    if (statement.rule.Equals("statement", StringComparison.InvariantCultureIgnoreCase) && (statement.children.Length == 1))
                    {
                        var command = statement.children[0];

                        if (command.rule.Equals("singleton", StringComparison.InvariantCultureIgnoreCase))
                        {
                            stmt.Singleton = QExplicitCommand.Create(stmt.Context, command);
                            stmt.IsValid = stmt.Singleton != null;
                            if (!stmt.IsValid)
                                stmt.Errors.Add("Unable to extract explicit command.");
                        }
                        else if (command.rule.Equals("implicits", StringComparison.InvariantCultureIgnoreCase))
                        {
                            stmt.Commands = QImplicitCommands.Create(stmt.Context, command, stmt);
                            stmt.IsValid = stmt.Commands != null;
                            if ((stmt.Errors.Count == 0) && !stmt.IsValid)
                            {
                                stmt.Errors.Add("Unable to extract implicit commands.");
                            }
                        }
                        else
                        {
                            stmt.IsValid = false;
                            stmt.Errors.Add("Unknown command type was encountered by parser.");
                        }
                    }
                }
                else
                {
                    stmt.IsValid = false;
                    if (!stmt.IsValid)
                        stmt.Errors.Add("Unable to identify a statement.");
                }
                return stmt;
            }
            return new QStatement() { Commands = null, Singleton = null, ParseDiagnostic = "", Errors = new() { "Unknown error: unable to perform statement parsing" }, Warnings = new(), IsValid = false, Text = "" };
        }

        public void AddError(string message)
        {
            this.Errors.Add(message);
        }

        public void AddWarning(string message)
        {
            this.Warnings.Add(message);
        }
        public void AddDisposition(string type, string message)
        {
            this.Disposition[type.ToLower()] = message;
        }
        private static PinshotLib PinshotDLL = new PinshotLib();

        public static (RootParse? pinshot, QStatement? blueprint, string fatal) Parse(string stmt, bool opaque = false, string? url = null)   // when url is unspecified, utilize pinvoke of pin-shot-avx.dll
        {
            (RootParse? pinshot, QStatement? blueprint, string fatal) root = (null, null, string.Empty);

            if ((url != null) && url.ToLower().StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                var svc = new PinshotSvc("http://127.0.0.1:3000/quelle");
                var task = svc.Parse(stmt);

                if (task.IsCompleted)
                {
                    root.pinshot = task.Result;
                }
            }
            else
            {
                root.pinshot = PinshotDLL.Parse(stmt).root;
            }
            if (root.pinshot != null)
            {
                QStatement blue = QStatement.Create(root.pinshot);
                if (blue.IsValid)
                {
                    root.blueprint = blue;
                }
                var error = root.pinshot.error;
                if (!string.IsNullOrEmpty(error))
                {
                    root.fatal = error;
                    Console.WriteLine(error);
                }
            }
            return root;
        }
    }
}
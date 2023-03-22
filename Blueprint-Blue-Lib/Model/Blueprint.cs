namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class Blueprint
    {
        public Blueprint(string avxpath = null)
        {
            ;
        }
        public QStatement Create(RootParse? root)
        {
            var env = new QEnvironment();

            if (root != null)
            {
                var stmt = new QStatement() { Commands = null, Singleton = null, ParseDiagnostic = root.error, Errors = new(), Warnings = new(), IsValid = true, Text = root.input };
                if (!string.IsNullOrEmpty(stmt.ParseDiagnostic))
                {
                    stmt.Errors.Add("See parse diagnostic for syntax errors.");
                    stmt.IsValid = false;
                }
                else if (root.result.Length == 1) // all parses in Quell should result in a single statement
                {
                    if (root.result[0].text.StartsWith('@'))
                    {
                        stmt.Singleton = Blueprint.CreateSingleton(env, root.result[0]);
                        stmt.IsValid = stmt.Singleton != null;
                        if (!stmt.IsValid)
                            stmt.Errors.Add("Unable to extract explicit command.");
                    }
                    else
                    {
                        stmt.Commands = Blueprint.CreateCommandVector(env, root.result[0], stmt);
                        stmt.IsValid = stmt.Commands != null;
                        if ((stmt.Errors.Count == 0) && !stmt.IsValid)
                        {
                            stmt.Errors.Add("Unable to extract implicit commands.");
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
        public static QExplicitCommand? CreateSingleton(QEnvironment env, Parsed stmt)
        {
            return QExplicitCommand.Create(env, stmt);
        }
        public static QImplicitCommands? CreateCommandVector(QEnvironment env, Parsed stmt, IStatement diagnostics)
        {
            return QImplicitCommands.Create(env, stmt, diagnostics);
        }
    }
}

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
                else
                {
                    if (root.result.Length == 1 && root.result[0].text.StartsWith('@'))
                    {
                        stmt.Singleton = Blueprint.CreateSingleton(env, root.result[0]);
                        stmt.IsValid = stmt.Singleton != null;
                    }
                    else
                    {
                        stmt.IsValid = false;
                    }
                }
                return stmt;
            }
            return new QStatement() { Commands = null, Singleton = null, ParseDiagnostic = "", Errors = new() { "Unknown error: unable to perform parse statement" }, Warnings = new(), IsValid = false, Text = "" };
        }
        public static QExplicitCommand? CreateSingleton(QEnvironment env, Parsed item)
        {
            return QExplicitCommand.Create(env, item);
        }
        public static QImplicitCommands? CreateCommandVector(QEnvironment env, Parsed[] items)
        {
            return null;
        }
    }
}

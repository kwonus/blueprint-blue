using BlueprintBlue;
using Pinshot.Blue;
using Pinshot.PEG;

namespace Blueprint.Blue
{
    public interface IStatement
    {
        void AddError(string message);
        void AddWarning(string message);

        QSettings GlobalSettings { get; set; }
        QSettings LocalSettings  { get; set; }
    }
    public class QStatement: IStatement
    {
        public string Text { get; set; }
        public bool IsValid { get; set; }
        public string ParseDiagnostic { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public QExplicitCommand? Singleton { get; set; }
        public QImplicitCommands? Commands { get; set; }
        public QContext Context { get; private set; }

        public QSettings GlobalSettings { get; set; }
        public QSettings LocalSettings { get; set; }

        public QStatement()
        {
            this.Text = string.Empty;
            this.IsValid= false;
            this.ParseDiagnostic= string.Empty;
            this.Errors = new();
            this.Warnings = new();
            this.Singleton = null;
            this.Commands= null;
            this.GlobalSettings = new QSettings(@"C:\Users\Me\AVX\Quelle\settings.quelle");
            this.LocalSettings = new QSettings(this.GlobalSettings);
            this.Context = new QContext(this, @"C:\Users\Me\AVX");  // notional placeholder for now (base this on actual username/home
        }

        public void AddError(string message)
        {
            this.Errors.Add(message);
        }

        public void AddWarning(string message)
        {
            this.Warnings.Add(message);
        }
        public QExpandableStatement? MaintainState()
        {
            return new QExpandableStatement(this);
        }
        private static PinshotLib PinshotDLL = new PinshotLib();

        public static (RootParse? pinshot, QStatement? blueprint, string fatal) Parse(string stmt, bool opaque = false, string? url = null)   // when url is unspecified, utilize pinvoke of pin-shot-avx.dll
        {
            (RootParse? pinshot, QStatement? blueprint, string fatal) root = (null, null, string.Empty);

            var blueprint = new Blueprint("");

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
                var blue = blueprint.Create(root.pinshot);
                if (blue.IsValid)
                {
                    if (!opaque)
                        blue.MaintainState();  // side-effects: AddHistory() & AddMacro()
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
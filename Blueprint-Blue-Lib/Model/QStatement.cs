namespace Blueprint.Blue
{
    using BlueprintBlue;
    using Pinshot.Blue;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using XBlueprintBlue;

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

        public QSettings GlobalSettings { get; set; }
        public QSettings LocalSettings { get; set; }

        public QStatement()
        {
            this.Text = string.Empty;
            this.IsValid= false;
            this.ParseDiagnostic= string.Empty;
            this.Errors = new();
            this.Warnings = new();
            this.Disposition = new();
            this.Singleton = null;
            this.Commands= null;
            this.GlobalSettings = new QSettings(@"C:\Users\Me\AVX\Quelle\settings.quelle");
            this.LocalSettings = new QSettings(this.GlobalSettings);
            this.Context = new QContext(this, @"C:\Users\Me\AVX");  // notional placeholder for now (base this on actual username/home
        }
        public XBlueprint Blueprint
        {
            get
            {
                if (this.IsValid)
                {
                    if (this.Singleton != null)
                        return this.Singleton.AsSingletonCommand();
                    if (this.Commands != null)
                        return this.Commands.AsSearchRequest();
                }
                return new XBlueprint()
                {
                    Settings = this.Context.AsMessage(),
                    Messages = this.Context.Statement.Errors.Count > 0 ? this.Context.Statement.Errors : new() { "Unexpected error or ill-defined request" },
                    Status = XStatusEnum.ERROR,
                    Help = "to be defined later"
                };
            }
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
namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public class QAbsorb : QSingleton, ICommand
    {
        public string Tag   { get; private set; } // macro
        public QContext Environment { get; private set; }
        public QAbsorb(QContext env, string text, Parsed[] args) : base(env, text, "use")
        {
            this.Tag = string.Empty;
            this.Environment = env;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            if (args.Length == 2 && args[0].rule == "use_cmd")
            {
                this.Tag = args[1].text;
            }
        }
        public override (bool ok, string message) Execute()
        {
            ExpandableInvocation? expandable = ExpandableHistory.Deserialize(tag: this.Tag);
            if (expandable == null)
                expandable = ExpandableMacro.Deserialize(tag: this.Tag);
            if (expandable != null)
            {
                var settings = expandable.Settings;
                foreach (var setting in expandable.Settings)
                {
                    QAssign assignment = QAssign.CreateAssignment(this.Environment, this.Text, setting.Key, setting.Value);
                    this.Environment.GlobalSettings.Assign(assignment);
                }
                this.Environment.GlobalSettings.Update();
                QGet get = new QGet(this.Environment, this.Text);
                return get.Execute();
            }
            return (false, "Hashtag not found.");
        }
    }
}
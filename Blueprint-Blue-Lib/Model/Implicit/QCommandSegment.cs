using BlueprintBlue.Model.Implicit;
using Pinshot.PEG;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Blueprint.Blue
{
    public class QCommandSegment : QCommand, ICommand
    {
        public QFind? SearchExpression      { get; internal set; }
//      public List<QCommand> Parts         { get; internal set; }
//      public List<QCommand> ExpandedParts { get; internal set; }
//      public string         ExpandedText  { get; internal set; }
        public List<QAssign>  Assignments   { get; internal set; }
        public List<QInvoke>  Invocations   { get; internal set; }
        public List<QFilter>  Filters       { get; internal set; }
        public List<QLimit>   Limitations   { get; internal set; }
        public QApply?        MacroLabel    { get; internal set; }

        public QSettings Settings { get; protected set; }

        public QCommandSegment(QContext env, string text, string verb, QApply? applyLabel = null) : base(env, text, verb)
        {
            this.Settings = new QSettings(env.GlobalSettings);

            this.SearchExpression = null;
            //this.Parts = new();
            //this.ExpandedParts = new();
            //this.ExpandedText = this.Text;
            this.Assignments = new();
            this.Invocations = new();
            this.Filters = new();
            this.Limitations = new();
            this.MacroLabel = applyLabel;
        }
        public static QCommandSegment? CreateSegment(QContext env, Parsed clause, QApply? applyLabel = null)
        {
            return new QCommandSegment(env, clause.text, clause.rule, applyLabel);
        }
    }
}
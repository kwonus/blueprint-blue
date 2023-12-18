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
        public static IEnumerable<QCommand> CreateComponents(QContext env, Parsed clause)
        {
            var command = clause.rule.Trim().ToLower();

            QCommand? result = null;

            switch (command)
            {
                case "invocation": result = QInvoke.Create(env, clause.text, clause.children); break; // invoke history/command or label/macro
                case "filter":     result = QFilter.Create(env, clause.text, clause.children); break;
                case "search":     result = QFind.Create(env, clause.text, clause.children);   break;
                case "apply":      result = QApply.Create(env, clause.text, clause.children);  break; // apply macro

                case "implicit_singletons":
                    {
                        foreach (var subordinate in clause.children)
                        {
                            var sub = subordinate.rule.Trim().ToLower();

                            switch (sub)
                            {
                                case "print": result = QLimit.Create(env, subordinate.text, subordinate.children); break;
                                case "export": result = QExport.Create(env, subordinate.text, subordinate.children); break;
                            }
                            if (result != null)
                            {
                                yield return result;
                            }
                        }
                    }
                    break;
            }
            if (result != null)
            {
                yield return result;
            }
            else
            {
                env.AddError("A command in the statement is ill-defined: " + clause.text);
            }
        }

        public static IEnumerable<QAssign> CreateVariables(QContext env, Parsed clause)
        {
            var command = clause.rule.Trim().ToLower();

            if (command == "opt")
            { 
                yield return QVariable.CreateAssignment(env, clause.text, clause.children);
            }
            else
            {
                env.AddError("A command in the statement is ill-defined: " + clause.text);
            }
        }
    }
}
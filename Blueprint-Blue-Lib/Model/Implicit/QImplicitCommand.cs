using BlueprintBlue.Model.Implicit;
using Pinshot.PEG;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Blueprint.Blue
{
    public abstract class QImplicitCommand : QCommand, ICommand
    {
        [JsonIgnore]
        [YamlIgnore]
        public QSettings Settings { get; protected set; }
        [JsonIgnore]
        [YamlIgnore]
        public bool IsExplicit { get => false; }
        public abstract string Expand();

        public QImplicitCommand(QContext env, string text, string verb) : base(env, text, verb)
        {
            this.Settings = new QSettings(env.GlobalSettings);
        }
        public static IEnumerable<QImplicitCommand> CreateSegments(QContext env, Parsed clause)
        {
            var command = clause.rule.Trim().ToLower();

            QImplicitCommand? result = null;

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
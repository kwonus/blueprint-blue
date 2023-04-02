﻿namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Runtime.CompilerServices;

    public class QImplicitCommands
    {
        public QContext Environment { get; set; }
        public string ExpandedText { get; set; }
        public List<QImplicitCommand> Parts { get; set; }
        public List<QImplicitCommand> ExpandedParts { get; set; }

        public IEnumerable<QFind> Searches
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QFind))
                        yield return (QFind)candidate;
            }
        }
        public IEnumerable<QFilter> Filters
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QFilter))
                        yield return (QFilter)candidate;
            }
        }
        public IEnumerable<QVariable> Assignments
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QSet))
                        yield return (QVariable)candidate;
                    else if (candidate.GetType() == typeof(QClear))
                        yield return (QVariable)candidate;
            }
        }
        public QMacro? Macro
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QMacro))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QMacro))
                            return (QMacro)candidate;
                return null;
            }
        }
        public QExport? Export
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QExport))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QExport))
                            return (QExport)candidate;
                return null;
            }
        }
        public QDisplay? Display
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QDisplay))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QDisplay))
                            return (QDisplay)candidate;
                return null;
            }
        }
        private QImplicitCommands(QContext env, string stmtText)
        {
            this.Environment = env;
            this.ExpandedText = stmtText;
            this.Parts = new List<QImplicitCommand>();

            this.ExpandedParts = new List<QImplicitCommand>();
        }

        public static QImplicitCommands? Create(QContext context, Parsed stmt, IStatement diagnostics)
        {
            bool valid = false;
            var commandSet = new QImplicitCommands(context, stmt.text);

            if (stmt.rule.Equals("statement", StringComparison.InvariantCultureIgnoreCase) && (stmt.children.Length == 1))
            {
                IPolarity? polarity = null;
                foreach (var command in stmt.children)
                {
                    if (command.rule.Equals("vector", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var clause in command.children)
                        {
                            if (clause.rule.Equals("negative", StringComparison.InvariantCultureIgnoreCase))
                            {
                                polarity = new QPolarityNegative(clause.text);
                                continue;
                            }
                            var objects = QImplicitCommand.Create(context, clause);
                            var test = false;

                            foreach (var obj in objects)
                            {
                                test = true;
                                if (obj.GetType() == typeof(QFind))
                                {
                                    if (polarity != null)
                                    {
                                        ((QFind)obj).Polarity = polarity;
                                        polarity = null;
                                    }
                                }
                                else if (polarity != null)
                                {
                                    context.AddError("A negative polarity was encountered, bit it did not proceed a find clause");
                                    polarity = null;
                                }
                                commandSet.Parts.Add(obj);
                            }
                            valid = test;
                            if (!valid)
                                //    break;
                                Console.WriteLine("Error clause; continuing for debugging purposes only!!!");
                        }
                    }
                }
            }
            if (valid)
            {
                // TO DO: Expand macros and invocations (cheat for now)
                commandSet.ExpandedParts = commandSet.Parts;
            }
            else
            {
                diagnostics.AddError("An command induced an unexpected error");
            }
            return valid ? commandSet : null;
        }
    }
}
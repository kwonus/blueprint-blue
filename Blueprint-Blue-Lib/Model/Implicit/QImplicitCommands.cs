﻿namespace Blueprint.Blue
{
    using XBlueprintBlue;
    using Pinshot.PEG;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using YamlDotNet.Serialization;
    using System.Text.Json.Serialization;

    public class QImplicitCommands
    {
        public QContext Context { get; set; }
        public string ExpandedText { get; set; }
        public List<QImplicitCommand> Parts { get; internal set; }
        public List<QImplicitCommand> ExpandedParts { get; internal set; }

        internal (int count, string stmt) Expand()    // count >= 0 means success and count of macros that were processed // -1 means there was an error
        {
            int hcount = 0;
            int mcount = 0;
            StringBuilder stmt = new();

            // for precedence to work in accordance with spec, expand macros and invocations first:
            //
            foreach (var part in this.Parts)
            {
                if (part == null)
                {
                    this.Context.AddWarning("Excountered an unexpected null value during statement expansion");
                    continue;
                }
                if (part.GetType() == typeof(QInvoke))
                {
                    hcount++;
                    var invoke = (QInvoke)part;
                    var expand = this.Context.Expand(invoke.Command);
                    if (expand != null)
                        stmt.Append(expand.Expansion);
                    else
                        this.Context.AddError("Unable to expand history item: $" + invoke.Command.ToString());
                }
                else if (part.GetType() == typeof(QInvoke))
                {
                    mcount++;
                    var utilize = (QInvoke)part;
                    var expand = this.Context.Expand(utilize.Label);
                    if (!string.IsNullOrEmpty(expand.Expansion))
                        stmt.Append(expand.Expansion);
                    else
                        this.Context.AddError("Unable to expand macro label: $" + utilize.Label);
                }
            }
            foreach (var part in this.Parts)
            {
                if (part == null)
                {
                    continue;
                }
                if (part.GetType() == typeof(QInvoke))
                {
                    ;
                }
                else
                {
                    stmt.Append(part.Text);
                }
            }
            return (hcount + mcount, stmt.ToString());
        }
        // TODO:
        public (int count, List<QImplicitCommand> result, string error) Compile()    // count >= 0 means success and count of macros that were processed // -1 means there was an error
        {
            (int count, List<QImplicitCommand> stmt, string error) result = (0, new(), "");

            foreach (var part in this.Parts)
            {
                if (part == null) continue;
                if (part.GetType() == typeof(QInvoke))
                {
                    ;
                }
                else
                {
                    ;
                }
            }
            return result;
        }
        [JsonIgnore]
        [YamlIgnore]
        public IEnumerable<QFind> Searches
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QFind))
                        yield return (QFind)candidate;
            }
        }
        [JsonIgnore]
        [YamlIgnore]
        public IEnumerable<QFilter> Filters
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QFilter))
                        yield return (QFilter)candidate;
            }
        }
        [JsonIgnore]
        [YamlIgnore]
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
        public QApply? Macro
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QApply))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QApply))
                            return (QApply)candidate;
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
                if (cnt >= 1) // TO DO: this should be == 1, but a bug is adding it twice
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QExport))
                            return (QExport)candidate;
                return null;
            }
        }
        public QLimit? Display
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QLimit))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QLimit))
                            return (QLimit)candidate;
                return null;
            }
        }
        private QImplicitCommands(QContext env, string stmtText)
        {
            this.Context = env;
            this.ExpandedText = stmtText;
            this.Parts = new List<QImplicitCommand>();

            this.ExpandedParts = new List<QImplicitCommand>();
        }

        public static QImplicitCommands? Create(QContext context, Parsed stmt, QStatement diagnostics)
        {
            bool valid = false;
            var commandSet = new QImplicitCommands(context, stmt.text);

            if (stmt.rule.Equals("statement", StringComparison.InvariantCultureIgnoreCase) && (stmt.children.Length == 1))
            {
                uint macro_cnt = 0;
                foreach (var command in stmt.children)
                {
                    if (command.rule.Equals("vector", StringComparison.InvariantCultureIgnoreCase)
                    || command.rule.Equals("macro_vector", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (Parsed clause in command.children)
                        {
                            if (clause.rule.Equals("invocation"))
                                macro_cnt++;
                        }
                    }
                }
                context.InvocationCount = macro_cnt;
                foreach (var command in stmt.children)
                {
                    if (command.rule.Equals("vector", StringComparison.InvariantCultureIgnoreCase)
                    ||  command.rule.Equals("macro_vector", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (Parsed clause in command.children)
                        {
                            var segments = QImplicitCommand.Create(context, clause);

                            foreach (var segment in segments)
                            {
                                commandSet.Parts.Add(segment);
                                valid = true;
                            }
                            if (!valid)
                                //    break;
                                Console.WriteLine("Error clause; continuing for debugging purposes only!!!");
                        }
                    }
                }
            }
            if (valid)
            {
                var expanded = commandSet.Expand();
                switch (expanded.count)
                {
                    case  0:    commandSet.ExpandedParts = commandSet.Parts;
                                commandSet.ExpandedText = stmt.text;
                                if (commandSet.Macro != null)
                                    commandSet.ExpandedText = commandSet.ExpandedText.Replace(commandSet.Macro.Text, "").Trim();
                                break;
                    case -1:
                                commandSet.ExpandedParts = commandSet.Parts;
                                commandSet.ExpandedText = stmt.text;
                                valid = false;
                                break;
                    default:    {   commandSet.ExpandedText = expanded.stmt;
                                    var expando = QStatement.Parse(expanded.stmt, opaque:true);
                                    if ((expando.blueprint != null) && expando.blueprint.IsValid && (expando.blueprint.Commands != null))
                                    {
                                        commandSet.ExpandedParts = expando.blueprint.Commands.Parts;
                                    }
                                    else
                                    {
                                        context.AddError("Unable to expand statement");
                                    }
                                }
                                break;
                }
            }
            else
            {
                diagnostics.AddError("A command induced an unexpected error");
            }
            return valid ? commandSet : null;
        }
        public XBlueprint AsSearchRequest()
        {
            var request = this.Context.Statement.IsValid && (this.Context.Statement.Errors.Count == 0)
            ? new XBlueprint()
            {
                Command = this.ExpandedText,
                Settings = this.Context.AsMessage(),
                Search = new List<XSearch>(),
                Status = XStatusEnum.OKAY,
                Help = "https://to-be-defined-later.html"
            }
            : new XBlueprint()
            {
                Command = this.ExpandedText,
                Settings = this.Context.AsMessage(),
                Search = new List<XSearch>(),
                Status = XStatusEnum.ERROR,
                Errors = this.Context.Statement.Errors.Count > 0 ? this.Context.Statement.Errors : new() { "Unknown error" },
                Help = "https://to-be-defined-later.html"
            };
            if (this.Context.Statement.Warnings.Count > 0)
            {
                request.Warnings = this.Context.Statement.Warnings;
            }

            if (this.Context.Statement.IsValid)
            {
                if (this.Filters.Any())
                {
                    request.Scope = new List<XScope>();
                    foreach (var detail in this.Filters)
                    {
                        request.Scope.Add(detail.AsMessage());
                    }
                }
                if (this.Searches.Any())
                {
                    foreach (var detail in this.Searches)
                    {
                        request.Search.Add(detail.AsMessage());
                    }
                }
            }
            return request;
        }
    }
}

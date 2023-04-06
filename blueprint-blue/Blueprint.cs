using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Collections.Generic;

namespace Blueprint.Blue
{
    public class QEnvironment
    {
        public string Format { get; set; }
        public string Scope { get; set; }
        public string User { get; set; }
        public string Session { get; set; }
    }
    public class QStatement
    {
        public string Session { get; set; }
        public string Text { get; set; }
        public bool IsValid { get; set; }
        public string Diagnostic { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public QExplicitCommand? Singleton { get; set; }
        public QImplicitCommands? Commands { get; set; }
    }

    public class QExplicitCommand: QCommand, ICommand
    {
        public bool IsExplicit { get => true; }

        public QExplicitCommand(QEnvironment env, string text) : base(env, text)
        {
            ;
        }
    }

    public class QImplicitCommand: QCommand, ICommand
    {
        public bool IsExplicit { get => false; }

        public QImplicitCommand(QEnvironment env, string text) : base(env, text)
        {
            ;
        }
    }

    public class QImplicitCommands
    {
        public QEnvironment Environment { get; set; }
        public string ExpandedText { get; set; }
        public List<QImplicitCommand> Parts { get; set; }
        public List<QImplicitCommand> ExpandedParts { get; set; }

        public IEnumerable<QFind> Searches
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QFind))
                        yield return (QFind) candidate;
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
                        cnt ++;
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
        
        public QImplicitCommands(QEnvironment env, string stmtText)
        {
            this.Environment = new QEnvironment();
            this.ExpandedText = stmtText;
            this.Parts = new List<QImplicitCommand>();
            this.ExpandedParts = new List<QImplicitCommand>();
        }
    }

    public interface ICommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public string Help { get; set; }
        public bool IsExplicit { get; }
    }

    public class QCommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public string Help { get; set; }
        public QEnvironment Env { get; protected set; }

        public QCommand(QEnvironment env, string text)
        {
            this.Env = env;
            this.Text = text;
        }
    }
    public class QHelpDoc
    {
        public string Verb { get; private set; }
        public string Document { get; private set; }

        private static Dictionary<string, QHelpDoc> Reference = new()
        {
            { "help",     new() { Verb = "@help",    Document = "Help"     } },
            { "exit",     new() { Verb = "@exit",    Document = "Exit"     } },
            { "version",  new() { Verb = "@version", Document = "Version"  } },
            { "delete",   new() { Verb = "@delete",  Document = "Macro"    } },
            { "expand",   new() { Verb = "@expand",  Document = "Macro"    } },
            { "review",   new() { Verb = "@review",  Document = "History"  } },
            { "get",      new() { Verb = "@get",     Document = "Settings" } },

            { "clear",    new() { Verb = "clear",    Document = "Settings" } },
            { "display",  new() { Verb = "display",  Document = "Display"  } },
            { "exec",     new() { Verb = "exec",     Document = "Exec"     } },
            { "export",   new() { Verb = "export",   Document = "Export"   } },
            { "filter",   new() { Verb = "filter",   Document = "Filter"   } },
            { "find",     new() { Verb = "find",     Document = "Find"     } },
            { "invoke",   new() { Verb = "invoke",   Document = "Macro"    } },
            { "macro",    new() { Verb = "macro",    Document = "Macro"    } },
            { "set",      new() { Verb = "set",      Document = "Settings" } },

            { "settings", new() { Verb = "",         Document = "Settings" } },
            { "history",  new() { Verb = "",         Document = "History"  } }

        };

        public static QHelpDoc GetDocument(string verb)
        {
            if (verb != null)
            {
                string key = verb.Trim().ToLower();
                if (key.StartsWith('@') && (key.Length > 1))
                    key = key.Substring(1);

                if (QHelpDoc.Reference.ContainsKey(key))
                    return QHelpDoc.Reference[key];
            }
            return QHelpDoc.Reference["help"];
        }
    }

    public class QHelp: QExplicitCommand, ICommand
    {
        public string Topic { get; set; }
        public QHelp(QEnvironment env, string text, string topic) : base(env, text)
        {
            this.Topic = topic;
        }
    }

    public class QGet: QExplicitCommand, ICommand
    {
        public string Key { get; set; }
        public QGet(QEnvironment env, string text, string key) : base(env, text)
        {
            this.Key = key;
        }
    }

    public class QDelete: QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QDelete(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }

    public class QExpand: QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QExpand(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }

    public class QVersion: QExplicitCommand, ICommand
    {
        public bool Verbose { get; set; }
        public QVersion(QEnvironment env, string text, bool verbose) : base(env, text)
        {
            this.Verbose = verbose;
        }
    }

    public class QReview: QExplicitCommand, ICommand
    {
        string[] Arguments { get; set; }
        public QReview(QEnvironment env, string text, string[] args) : base(env, text)
        {
            this.Arguments = args;
        }
    }

    public class QExit: QExplicitCommand, ICommand
    {
        public QExit(QEnvironment env, string text) : base(env, text)
        {
            ;
        }
    }

    public class QFind: QImplicitCommand, ICommand
    {
        public bool IsQuoted { get; set; }
        public List<QSearchSegment> Segments { get; set; }

        public QFind(QEnvironment env, string text) : base(env, text)
        {
            this.IsQuoted = false;
            this.Segments = new();
        }
    }

    public class QSearchSegment
    {
        private string Text;
        public List<QSearchFragment> Fragments { get; set; }

        public QSearchSegment(string text)
        {
            this.Text = text;
            this.Fragments = new();
        }
    }

    public class QSearchFragment
    {
        private string Text;
        public List<ITerm> Terms { get; set; }
        public QSearchFragment(string text)
        {
            this.Text = text;
            this.Terms = new();
        }
    }

    public interface ITerm
    {
        public string Text { get; set; }
    }

    public class QuelleTerm : ITerm
    {
        public string Text { get; set; }
        public QuelleTerm(string text)
        {
            this.Text = text;
        }
    }

    public class QWord : QuelleTerm, ITerm
    {
        public int WordKey { get; set; }

        public QWord(string text): base(text)
        {
            ;
        }
    }

    public class QMatch : QuelleTerm, ITerm
    {
        public string Beginning { get; set; }
        public string Ending { get; set; }

        public QMatch(string text) : base(text)
        {
            this.Beginning = string.Empty;
            this.Ending = string.Empty;
        }
    }

    public class QLemma : QuelleTerm, ITerm
    {
        public int LemmaKey { get; set; }

        public QLemma(string text) : base(text)
        {
            this.Text = text;
        }
    }

    public class QPunc: QuelleTerm, ITerm
    {
        public int Punctuation { get; set; }

        public QPunc(string text) : base(text)
        {
            this.Punctuation = 0;
        }
    }

    public class QDecor: QuelleTerm, ITerm
    {
        public int Decoration { get; set; }

        public QDecor(string text) : base(text)
        {
            this.Decoration = 0;
        }
    }

    public class QPos: QuelleTerm, ITerm
    {
        public int PnPos12 { get; set; }
        public int Pos32 { get; set; }

        public QPos(string text) : base(text)
        {
            this.PnPos12 = 0;
            this.Pos32 = 0;
        }
    }

    public class QLoc: QuelleTerm, ITerm
    {
        public int Position { get; set; }

        public QLoc(string text) : base(text)
        {
            this.Position = 0;
        }
    }

    public class QFilter: QImplicitCommand, ICommand
    {
        public string Scope { get; set; }

        public QFilter(QEnvironment env, string text, string scope) : base(env, text)
        {
            this.Scope = scope;
        }
    }
    public class QVariable : QImplicitCommand, ICommand
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }

        public QVariable(QEnvironment env, string text, string key, string value) : base(env, text)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public class QSet: QVariable, ICommand
    {
        public QSet(QEnvironment env, string text, string key, string value) : base(env, text, key, value)
        {
            ;
        }
    }

    public class QClear: QVariable, ICommand
    {
        public QClear(QEnvironment env, string text, string key) : base(env, text, key, string.Empty)
        {
            ;
        }
    }

    public class QMacro: QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        public QMacro(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }

    public class QExport: QImplicitCommand, ICommand
    {
        public string FileSpec { get; set; }

        public QExport(QEnvironment env, string text, string spec) : base(env, text)
        {
            this.FileSpec = spec;
        }
    }

    public class QDisplay: QImplicitCommand, ICommand
    {
        public List<uint> Fields { get; set; }

        public QDisplay(QEnvironment env, string text, IEnumerable<uint> fields) : base(env, text)
        {
            foreach (var field in fields)
                this.Fields.Add(field);
        }
    }

    public class QInvoke: QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        public QInvoke(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }

    public class QExec: QImplicitCommand, ICommand
    {
        public uint Command { get; set; }

        public QExec(QEnvironment env, string text, uint command) : base(env, text)
        {
            this.Command = command;
        }
    }
    public class Query
    {
        public QStatement Result(string statement) => new QStatement();
    }

    public class ManagementResult
    {
        public bool Success { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Errors { get; set; }
        public string Session { get; set; }
    }

    public class Manage
    {
        public ManagementResult Status(string session, ManagementOperation op, string passcode) => new ManagementResult();
    }

    public enum ManagementOperation
    {
        ESTABLISH_SESSION,
        RESUME_SESSION,
        REVOKE_SESSION,
        CLEAR_SESSION_HISTORY,
        RESET_SESSION
    }
}
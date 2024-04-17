namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Results;
    using AVXLib.Framework;
    using Blueprint.Blue;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using YamlDotNet.Serialization;
    using static Blueprint.Model.Implicit.QFormat;
    using static System.Net.Mime.MediaTypeNames;

    public enum FileCreateMode
    {
        CreateNew = 0,
        Overwrite = 1,
        Append = 2
    }
    public record Word(string Text, string Modernized, Written Features);
    public abstract class ExportDirective : Dictionary<byte, Dictionary<byte, Dictionary<byte, Word>>> // members of this class are very opaque for serialization purposes
    {
        protected string FileSpec { get; set; }
        protected FileCreateMode CreationMode { get; set; }
        protected QFormatVal ContentFormat { get; set; }

        public (DirectiveResultType status, QFormatVal format, FileCreateMode mode) Export(SelectionResultType selectionResultType, QSelectionStatement selection)
        {
            this.Clear();

            DirectiveResultType resultType = this.Deserialize();
            return (
                resultType != DirectiveResultType.ExportFailed
                ? this.Serialize(selectionResultType, selection)
                : resultType,
                this.ContentFormat, this.CreationMode);
        }
        protected abstract DirectiveResultType Deserialize();
        protected abstract DirectiveResultType Serialize(SelectionResultType selectionResultType, QSelectionStatement selection);

        protected ExportDirective(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode): base()
        {
            this.FileSpec = spec;
            this.CreationMode = mode;
            this.ContentFormat = format;
        }
        public static ExportDirective? Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1 && args[0].children.Length == 1 && args[0].children[0].rule.Equals("filename", StringComparison.InvariantCultureIgnoreCase))
            {
                string spec = args[0].children[0].text;

                if (env.Statement.Commands != null && env.Statement.Commands.SelectionCriteria != null)
                {
                    FileCreateMode mode = FileCreateMode.CreateNew;
                    QFormat format = env.Statement.Commands.SelectionCriteria.Settings.Format;

                    if (args[0].rule.Equals("overwrite", StringComparison.InvariantCultureIgnoreCase))
                        mode = FileCreateMode.Overwrite;
                    else if (args[0].rule.Equals("append", StringComparison.InvariantCultureIgnoreCase))
                        mode = FileCreateMode.Append;

                    switch (format.Value)
                    {
                        case QFormatVal.JSON: return new ExportHtml(env, text, spec, format.Value, mode);
                        case QFormatVal.YAML: return new ExportYaml(env, text, spec, format.Value, mode);
                        case QFormatVal.TEXT: return new ExportText(env, text, spec, format.Value, mode);
                        case QFormatVal.HTML: return new ExportHtml(env, text, spec, format.Value, mode);
                        case QFormatVal.MD:   return new ExportMarkdown(env, text, spec, format.Value, mode);
                    }
                }
            }
            return null;
        }
    }
    public class ExportJson : ExportDirective
    {
        internal ExportJson(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        protected override DirectiveResultType Deserialize()
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec)))
            {
                try
                {
                    if (this.CreationMode != FileCreateMode.CreateNew && File.Exists(this.FileSpec))
                    {
                        using (var file = File.OpenText(this.FileSpec))
                        {
                            //foreach (string line in lines)
                            {
                                //break; // figure out if input is json or yaml ... close file and deserialize accordingly
                            }
                        }
                    }
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
        protected override DirectiveResultType Serialize(SelectionResultType selectionResultType, QSelectionStatement selection)
        {
            return DirectiveResultType.ExportFailed;
        }
    }
    public class ExportYaml : ExportDirective
    {
        internal ExportYaml(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        protected override DirectiveResultType Deserialize()
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec)))
            {
                try
                {
                    if (this.CreationMode != FileCreateMode.CreateNew && File.Exists(this.FileSpec))
                    {
                        using (var file = File.OpenText(this.FileSpec))
                        {
                            //foreach (string line in lines)
                            {
                                //break; // figure out if input is json or yaml ... close file and deserialize accordingly
                            }
                        }
                    }
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
        protected override DirectiveResultType Serialize(SelectionResultType selectionResultType, QSelectionStatement selection)
        {
            return DirectiveResultType.ExportFailed;
        }
    }
    public class ExportText : ExportDirective
    {
        private TextWriter? Writer;
        internal ExportText(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            this.Writer = null;
        }
        protected override DirectiveResultType Deserialize()
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec)))
            {
                try
                {
                    if (this.CreationMode != FileCreateMode.CreateNew && File.Exists(this.FileSpec))
                    {
                        this.Writer = this.CreationMode == FileCreateMode.Append
                             ? File.AppendText(this.FileSpec)
                             : File.CreateText(this.FileSpec);
                    }
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
        protected override DirectiveResultType Serialize(SelectionResultType selectionResultType, QSelectionStatement selection)
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (this.Writer != null)
            {
                try
                {
                    result = DirectiveResultType.ExportSuccessful;

                    if (selectionResultType == SelectionResultType.SearchResults)
                    {

                    }
                    else if (selectionResultType == SelectionResultType.ScopeOnlyResults)
                    {

                    }
                    this.Writer.Flush();
                    this.Writer.Close();
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
    }
    public class ExportHtml : ExportDirective
    {
        private TextWriter? Writer;
        internal ExportHtml(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            this.Writer = null;
        }
        protected override DirectiveResultType Deserialize()
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec)))
            {
                try
                {
                    if (this.CreationMode != FileCreateMode.CreateNew && File.Exists(this.FileSpec))
                    {
                        this.Writer = this.CreationMode == FileCreateMode.Append
                             ? File.AppendText(this.FileSpec)
                             : File.CreateText(this.FileSpec);
                    }
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
        protected override DirectiveResultType Serialize(SelectionResultType selectionResultType, QSelectionStatement selection)
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (this.Writer != null)
            {
                try
                {
                    result = DirectiveResultType.ExportSuccessful;

                    if (selectionResultType == SelectionResultType.SearchResults)
                    {

                    }
                    else if (selectionResultType == SelectionResultType.ScopeOnlyResults)
                    {

                    }
                    this.Writer.Flush();
                    this.Writer.Close();
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
    }
    public class ExportMarkdown : ExportDirective
    {
        private TextWriter? Writer;
        internal ExportMarkdown(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            this.Writer = null;
        }
        protected override DirectiveResultType Deserialize()
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec)))
            {
                try
                {
                    if (this.CreationMode != FileCreateMode.CreateNew && File.Exists(this.FileSpec))
                    {
                        this.Writer = this.CreationMode == FileCreateMode.Append
                             ? File.AppendText(this.FileSpec)
                             : File.CreateText(this.FileSpec);
                    }
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
        protected override DirectiveResultType Serialize(SelectionResultType selectionResultType, QSelectionStatement selection)
        {
            DirectiveResultType result = DirectiveResultType.ExportFailed;

            if (this.Writer != null)
            {
                try
                {
                    result = DirectiveResultType.ExportSuccessful;

                    if (selectionResultType == SelectionResultType.SearchResults)
                    {

                    }
                    else if (selectionResultType == SelectionResultType.ScopeOnlyResults)
                    {

                    }
                    this.Writer.Flush();
                    this.Writer.Close();
                }
                catch
                {
                    result = DirectiveResultType.ExportFailed;
                }
            }
            return result;
        }
    }
}
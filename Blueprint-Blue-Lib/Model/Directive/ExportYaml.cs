namespace Blueprint.Blue
{
    using YamlDotNet.Serialization;
    using AVXLib;
    using System;
    using static AVXLib.Framework.Numerics;
    using static Blueprint.Model.Implicit.QFormat;
    using System.Drawing;

    public class ExportYaml : ExportDirective
    {
        protected override bool UsesAugmentation { get => true; }

        internal ExportYaml(QContext env, string spec, FileCreateMode mode) : base(env, spec, QFormatVal.YAML, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            var book = ObjectTable.AVXObjects.Mem.Book.Slice(0, 67).Span;

            TextWriter? writer = null;
            try
            {
                writer = (this.CreationMode == FileCreateMode.Streaming)
                    ? this.Context != null ? new StreamWriter(this.Context.InternalExportStream) : null
                    : File.CreateText(this.FileSpec);

                if (writer != null)
                {
                    var serializer = new SerializerBuilder();
                    var builder = serializer.Build();

                    foreach (byte b in from bk in this.Keys orderby bk select bk)
                    {
                        string name = "- " + book[b].abbr4.ToString() + " ";
                        foreach (byte c in from ch in this[b].Keys orderby ch select ch)
                        {
                            string chap = name + c.ToString() + "|";
                            foreach (byte v in from vs in this[b][c].Keys orderby vs select vs)
                            {
                                string coord = chap + v.ToString() + ":";
                                writer.WriteLine(coord);

                                string yaml = builder.Serialize(this[b][c][v]);

                                string[] lines = yaml.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string line in lines)
                                {
                                    writer.Write("  ");
                                    writer.WriteLine(line);
                                }
                            }
                        }
                    }
                    if (writer != null)
                    {
                        writer.Flush();
                        if (this.CreationMode != FileCreateMode.Streaming)
                            writer.Close();
                    }
                    return DirectiveResultType.ExportSuccessful;
                }
            }
            catch
            {
                ;
            }
            if (writer != null)
            {
                writer.Close();
            }
            return DirectiveResultType.ExportFailed;
        }
    }
}
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

        internal ExportYaml(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            var book = ObjectTable.AVXObjects.Mem.Book.Slice(0, 67).Span;

            try
            {
                using (TextWriter writer = File.CreateText(this.FileSpec))
                {
                    var serializer = new SerializerBuilder();
                    var builder = serializer.Build();

                    foreach (byte b in from bk in this.Keys orderby bk select bk)
                    {
                        string name = book[b].abbr4.ToString() + " ";
                        foreach (byte c in from ch in this[b].Keys orderby ch select ch)
                        {
                            string chap = name + c.ToString() + "|";
                            foreach (byte v in from vs in this[b][c].Keys orderby vs select vs)
                            {
                                string coord = chap + v.ToString() + ":";
                                writer.WriteLine(coord);

                                string yaml = builder.Serialize(this[b][c][v]);
                                writer.Write(yaml);
                            }
                        }
                    }
                    return DirectiveResultType.ExportSuccessful;
                }
            }
            catch
            {
                ;
            }

            return DirectiveResultType.ExportFailed;
        }
    }
}
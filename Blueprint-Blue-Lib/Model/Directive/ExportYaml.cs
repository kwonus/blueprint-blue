namespace Blueprint.Blue
{
    using YamlDotNet.Serialization;
    using AVXLib;
    using System;
    using static AVXLib.Framework.Numerics;
    using static Blueprint.Model.Implicit.QFormat;

    public class ExportYaml : ExportDirective
    {
        protected override bool UsesAugmentation { get => true; }

        internal ExportYaml(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            try
            {
                using (TextWriter writer = File.CreateText(this.FileSpec))
                {
                    var serializer = new SerializerBuilder();
                    var builder = serializer.Build();

                    string yaml = builder.Serialize(this);
                    writer.Write(yaml);

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
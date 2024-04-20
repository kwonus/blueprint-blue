namespace Blueprint.Blue
{
    using AVXLib;
    using System;
    using static AVXLib.Framework.Numerics;
    using static Blueprint.Model.Implicit.QFormat;

    public class ExportJson : ExportDirective
    {
        protected override bool UsesAugmentation { get => true; }

        public ExportJson(): base()
        {
            ;
        }
        internal ExportJson(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            try
            {
                using (Stream stream = File.Create(this.FileSpec))
                {
                    System.Text.Json.JsonSerializer.Serialize<ExportJson>(stream, this);
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
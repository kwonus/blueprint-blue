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
        internal ExportJson(QContext env, string spec, FileCreateMode mode) : base(env, spec, QFormatVal.JSON, mode)
        {
            ;
        }
        // JSON is no longer suppored
        public override DirectiveResultType Update()
        {
            return DirectiveResultType.ExportFailed;
        }
    }
}
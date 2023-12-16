namespace Blueprint.Blue
{
    using BlueprintBlue.Model.Implicit;
    public class QAssign : QVariable
    {
        public QAssign(QContext env, string text, string key, string value) : base(env, text, "assign", key, value)
        {
            ;
        }
    }
}
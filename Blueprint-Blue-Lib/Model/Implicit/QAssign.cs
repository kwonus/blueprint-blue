namespace Blueprint.Blue
{
    using Blueprint.Model.Implicit;
    public class QAssign : QVariable
    {
        public QAssign(QContext env, string text, string key, string value) : base(env, text, "assign", key, value)
        {
            ;
        }
    }
}
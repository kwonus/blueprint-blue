namespace Blueprint.Blue
{
    public class QClear : QVariable, ICommand
    {
        public override string Expand()
        {
            return this.Text;
        }

        public QClear(QContext env, string text, string key, bool persistent) : base(env, text, "clear", key, persistent)
        {
            ;
        }
    }
}
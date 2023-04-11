namespace Blueprint.Blue
{
    public class QCurrent : QVariable, ICommand
    {
        public override string Expand()
        {
            return this.Text;
        }

        public QCurrent(QContext env, string text, string key, bool persistent) : base(env, text, "settings", key, persistent)
        {
            ;
        }
    }
}
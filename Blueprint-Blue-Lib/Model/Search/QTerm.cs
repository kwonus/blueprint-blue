using Pinshot.PEG;

namespace Blueprint.Blue
{
    public interface ITerm
    {
        public string Text { get; }
    }

    public class QTerm : ITerm
    {
        public string Text { get; private set; }
        public Parsed Parse { get; private set; }
        protected QTerm(string text, Parsed parse)
        {
            this.Text = text;
            this.Parse = parse;
        }
        public static QTerm? Create(string text, Parsed parse)
        {
            return new QTerm(text, parse);  // This factory method should only return child classes [not QTerm]
        }
    }
}
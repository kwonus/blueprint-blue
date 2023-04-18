namespace Blueprint.Blue
{
    public interface IPolarity
    {
        bool Positive { get; }
        string Text { get; }
        string AsYaml();
    }

    public class QPolarityPositive : IPolarity
    {
        public bool Positive
        {
            get => true;
        }
        public string Text
        {
            get => string.Empty;
        }
        private QPolarityPositive()
        {
            ;
        }
        public readonly static QPolarityPositive POLARITY_DEFAULT = new QPolarityPositive();

        public string AsYaml()
        {
            return "negate: false";
        }
    }

    public class QPolarityNegative : IPolarity
    {
        public string Text { get; private set; }

        public QPolarityNegative(string text)
        {
            this.Text = text;
        }
        public bool Positive
        {
            get => false;
        }
        public string AsYaml()
        {
            return "negate: true";
        }
    }
}
using Blueprint.Blue;
using System.Text;

namespace BlueprintBlue
{
    public class QExpandableStatement
    {
        public Int64 Time             { get; set; }
        public string Statement       { get; set; }
        public string Expansion       { get; set; }

        public QExpandableStatement()
        {
            this.Time = 0;
            this.Statement = string.Empty;
            this.Expansion = string.Empty;
        }

        public QExpandableStatement(QStatement statement)
        {
            this.Time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this.Statement = statement.IsValid ? statement.Text : string.Empty;
            this.Expansion = string.Empty;

            if (statement.IsValid)
            {
                bool isMacroDef = (statement.Commands != null) && (statement.Commands.Macro != null);
                var expandable = new StringBuilder();
                if (statement.Commands != null)
                {
                    this.Expansion = statement.Commands.ExpandedText;
                    int i = 0;
                    foreach (var clause in statement.Commands.Assignments)
                    {
                        if (++i > 1)
                            expandable.Append(' ');
                        expandable.Append(clause.Expand());
                    }
                    foreach (var clause in statement.Commands.Searches)
                    {
                        var expansion = clause.Expand();
                        if (++i > 1)
                        {
                            if (expansion.StartsWith("--"))
                                expandable.Append(" + ");
                            else
                                expandable.Append(' ');
                        }
                        expandable.Append(expansion);
                    }
                    foreach (var clause in statement.Commands.Filters)
                    {
                        if (++i > 1)
                            expandable.Append(' ');
                        expandable.Append(clause.Expand());
                    }
                }
                this.Expansion = expandable.ToString();

                if (statement.Commands != null && statement.Commands.Macro != null)
                {
                    if (isMacroDef)
                        statement.Context.AddMacro(this, statement.Commands.Macro.Label);
                }
                else
                {
                    statement.Context.AddError("This method should not be called with an invalid statement");
                }
            }
        }

        public DateTime GetDateTime()
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(this.Time);
            return offset.DateTime;
        }

        // Operator overloading:
        //
        public static bool operator == (QExpandableStatement? q1, QExpandableStatement? q2)
        {
            if ((object)q1 == null || (object)q2 == null)
                return false;

            return q1.Equals(q2);
        }

        public static bool operator !=(QExpandableStatement? q1, QExpandableStatement? q2)
        {
            if ((object)q1 == null || (object)q2 == null)
                return false;

            return !(q1 == q2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
                return false;

            var q2 = (QExpandableStatement)obj;
            return this.Statement == q2.Statement;
        }

        public override int GetHashCode()
        {
            return this.Statement.GetHashCode();
        }
    }
}

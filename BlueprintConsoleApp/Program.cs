namespace BlueprintConsoleApp
{
    using BlueprintBlue;
    using BlueprintBlue.PEG;

    internal class Program
    {
        static void Print(IEnumerable<Parsed> parses, int level)
        {
            foreach(var parse in parses)
            {
                int paren = parse.rule.IndexOf('(');
                string rule = paren > 0 ? parse.rule.Substring(0, paren).Trim() : parse.rule.Trim();
                for (int i = 0; i < level; i++)
                    Console.Write('\t');

                Console.WriteLine(rule + ":\t" + parse.text.Trim());

                if (parse.children.Length > 0)
                {
                    Program.Print(parse.children, level + 1);
                }
            }
        }
        static void Main(string[] args)
        {
            var blueprint = new Blueprint("http://127.0.0.1:3000/quelle");
            var task = blueprint.Parse("\"\\foo\\ ... [he said] ... /pronoun/&/3p/\" + bar + x|y&z a&b&c > xfile < genesis 1:1");

            if (task.IsCompleted)
            {
                var result = task.Result;

                if (result != null)
                {
                    var error = result.error;
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine(error);
                    }
                    else
                    {
                        var list = result.result;
                        Program.Print(list, 0);
                    }
                }
            }
        }
    }
}
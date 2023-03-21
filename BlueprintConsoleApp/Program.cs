namespace BlueprintConsoleApp
{
    using Pinshot.Blue;
    using Pinshot.PEG;
    using Blueprint.Blue;

    internal class Program
    {
        static void Print(IEnumerable<Parsed> parses, int level)
        {
            foreach(var parse in parses)
            {
                int paren = parse.rule.IndexOf('(');
                string rule = paren > 0 ? parse.rule.Substring(0, paren).Trim() : parse.rule.Trim();
                for (int i = 0; i < level; i++)
                    Console.Write("  ");

                Console.WriteLine(rule + ":\t" + parse.text.Trim());

                if (parse.children.Length > 0)
                {
                    Program.Print(parse.children, level + 1);
                }
            }
        }
//      const string TestStmt = "\"\\foo\\ ... [he said] ... /pronoun/&/3p/\" + bar + x|y&z a&b&c > xfile < genesis 1:1";
        const string TestStmt = "@Help find";

        static void Main(string[] args)
        {
            string? url = (args != null) && (args.Count() >= 1) && (args[0] != null) ? args[0] : null; // e.g. "http://127.0.0.1:3000/quelle"

            RootParse? root = null;

            var blueprint = new Blueprint("");


            if ((url != null) && url.ToLower().StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                var svc = new PinshotSvc("http://127.0.0.1:3000/quelle");
                var task = svc.Parse(TestStmt);

                if (task.IsCompleted)
                {
                    root = task.Result;
                }
            }
            else
            {
                var lib = new PinshotLib();
                var result = lib.Parse(TestStmt);
                root = result.root;
            }
            var blue = blueprint.Create(root);
            if (root != null)
            {
                var error = root.error;
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine(error);
                }
                else
                {
                    var list = root.result;
                    Program.Print(list, 0);
                }
            }
        }
    }
}
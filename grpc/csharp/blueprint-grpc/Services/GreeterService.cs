using blueprint_grpc;
using Grpc.Core;

namespace blueprint_grpc.Services
{
    public class ParseStatement : ParseStatement.ParseStatementBase
    {
//      private readonly ILogger<ParseStatement> _logger;
        public ParseStatement()//ILogger<ParseStatement> logger)
        {
            ;// _logger = logger;
        }

        public override Task<ParseResponse> Parse(ParseRequest request, ServerCallContext context)
        {
            var response = new ParseResponse();
            response.Input = "Foo";
            response.Output = new QStatement();
            response.Output.Text = "Bar";

            var command = new QCommand();
            command.Item = new QExplicitCommand();
            command.Item.Help = new QExplicitCommand.Types.QHelp();
            command.Item.Help.Topic = "Exit";

            response.Output.Commands.Add(command);

            return Task.FromResult(response);
        }
    }
}
use blueprint::parse_statement_server::ParseStatement;
use tonic::{transport::Server, Request, Response, Status};
use crate::blueprint::{ParseRequest, ParseResponse, QStatement, QExplicitCommand, QHelp, q_statement, q_explicit_command};
use crate::blueprint::parse_statement_server::ParseStatementServer;

mod blueprint;

#[derive(Debug, Default)]
pub struct BlueprintBlueService {}

#[tonic::async_trait]
impl ParseStatement for BlueprintBlueService {
    async fn parse(
        &self,
        request: tonic::Request<ParseRequest>,
    ) -> Result<tonic::Response<ParseResponse>, tonic::Status> {

        let r = request.into_inner();
        let result = Ok(Response::new(ParseResponse {
            input: String::from("foo"),
            output: Some(QStatement {
                text: String::from("foo"),
                is_valid: true,
                is_explicit: true,
                message: String::from(""),
                directives: Some(q_statement::Directives::Explicit(QExplicitCommand {
                    text: String::from("foo"),
                    verb: String::from("@Help"),
                    topic: String::from("help"),
                    command: Some(q_explicit_command::Command::Help(QHelp {
                        topic: r.statement_text,
                    })),
                })),
            }),
            error_lines: vec![],
        }));
        result
    }
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let address = "[::1]:8080".parse().unwrap();
    let service = BlueprintBlueService::default();

    Server::builder().add_service(ParseStatementServer::new(service))
        .serve(address)
        .await?;
    Ok(())

}
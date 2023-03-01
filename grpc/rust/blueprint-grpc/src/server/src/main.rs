use tonic::{transport::Server, Request, Response, Status};
use avx_quelle::{ParseRequest, ParseResponse, avx_quelle_server::{ParseStatement, ParseStatementServer}};

pub mod avx_quelle {
    tonic::include_proto!("blueprint");
}

#[derive(Debug, Default)]
pub struct ParseStatement {}

#[tonic::async_trait]
impl Parse for ParseStatement {
    async fn parse(&self, request: Request<ParseRequest>) -> Result<Response<ParseResponse>, Status> {
        let r = request.into_inner();
        match r.StatementText {
            0 => Ok(Response::new(voting::VotingResponse { confirmation: {
                format!("Happy to confirm that you upvoted for {}", r.url)
            }})),
            1 => Ok(Response::new(voting::VotingResponse { confirmation: {
                format!("Confirmation that you downvoted for {}", r.url)
            }})),
            _ => Err(Status::new(tonic::Code::OutOfRange, "Invalid vote provided"))
        }
    }
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let address = "[::1]:8080".parse().unwrap();
    let voting_service = VotingService::default();

    Server::builder().add_service(VotingServer::new(voting_service))
        .serve(address)
        .await?;
    Ok(())

}
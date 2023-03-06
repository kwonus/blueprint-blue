use crate::blueprint::parse_statement_server::ParseStatement;
use crate::blueprint::{ParseRequest, ParseResponse, QStatement, QExplicitCommand};

mod blueprint;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let mut client = parse_statement_client::connect("http://[::1]:8080").await?;
    loop {
        println!("\nPlease vote for a particular url");/*
        let mut u = String::new();
        println!("Please type a statement: ");
        stdin().read_line(&mut u).unwrap();
        let u = u.trim();
        let blueprint::ParseRequest::new() = { StatementText: u };
        let response = client.parse(request).await?;
        println!("Got: '{}' from service", response.into_inner().confirmation);*/
    }
    Ok(())
}
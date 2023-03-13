use std::io::stdin;
use crate::blueprint::blueprint_blue_server::BlueprintBlue;
use crate::blueprint::{ParseRequest, ParseResponse, QStatement, QExplicitCommand, blueprint_blue_client};

mod blueprint;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let mut client = blueprint_blue_client::BlueprintBlueClient::connect("http://[::1]:8080").await?;
    loop {
        let mut u = String::new();
        println!("Please type a statement: ");
        stdin().read_line(&mut u).unwrap();
        let u = u.trim();
        let request = blueprint::ParseRequest { statement_text: String::from(u) };
        let response = client.parse(request).await?;
        let output = response.into_inner().output;
        if output.is_some() {
            println!("Got: '{}' from service", output.unwrap().text);
        }
    }
    Ok(())
}
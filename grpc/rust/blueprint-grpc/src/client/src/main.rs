use tonic::{transport::Server, Request, Response, Status};

pub mod avx_quelle {
    tonic::include_proto!("blueprint");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let mut client = ParseClient::connect("http://[::1]:8080").await?;
    loop {
        println!("\nPlease vote for a particular url");
        let mut u = String::new();
        println!("Please type a statement: ");
        stdin().read_line(&mut u).unwrap();
        let u = u.trim();
        let avx_quelle::ParseRequest::new() = { StatementText: u };
        let response = client.parse(request).await?;
        println!("Got: '{}' from service", response.into_inner().confirmation);
    }
    Ok(())
}
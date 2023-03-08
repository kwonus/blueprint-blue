use std::collections::HashMap;
///////////////
// Blueprint //
///////////////
use blueprint::blueprint_blue_server::BlueprintBlue;
use tonic::{transport::Server, Request, Response, Status};
use crate::blueprint::{ParseRequest, ParseResponse, QStatement, QExplicitCommand, QHelp, q_statement, q_explicit_command};
use crate::blueprint::blueprint_blue_server::BlueprintBlueServer;

mod blueprint;

#[derive(Debug, Default)]
pub struct BlueprintBlueService {}

//////////////
// Pin-Shot //
//////////////
use serde::{Deserialize, Serialize};
extern crate pest;
#[macro_use]
extern crate pest_derive;

use std::any::Any;
use pest::Parser;
use pest::iterators::Pairs;

#[derive(Parser)]
#[grammar = "avx-quelle.pest"]
struct QuelleParser;

#[derive(Serialize)]
struct Parsed {
    rule: String,
    text: String,
    children: Vec<Parsed>,
}

#[derive(Deserialize)]
struct QuelleStatement {
    statement: String,
}

#[derive(Serialize)]
struct RootParse {
    input: String,
    result: Vec<Parsed>,
    error: String,
}

#[tonic::async_trait]
impl BlueprintBlue for BlueprintBlueService {
    async fn parse(
        &self,
        request: tonic::Request<ParseRequest>,
    ) -> Result<tonic::Response<ParseResponse>, tonic::Status> {

        let req = request.into_inner();
        let stmt = req.statement_text.clone();

        let mut pinshot = RootParse {
            input: req.statement_text,
            result: vec![],
            error: "".to_string(),
        };

        let task = QuelleParser::parse(Rule::command, &stmt);

        if task.is_ok() {
            let pairs = task.unwrap();
            pinshot_recurse(pairs, &mut pinshot.result);
        }
        else if task.is_err() {
            pinshot.error = task.unwrap_err().to_string();
        }
        else {
            pinshot.error = "internal error".to_string();
        }

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
                        topic: String::from("bar"),
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

    Server::builder().add_service(BlueprintBlueServer::new(service))
        .serve(address)
        .await?;
    Ok(())

}

fn pinshot_recurse(children: Pairs<Rule>, items: &mut Vec<Parsed>)
{
    for pair in children {
        let mut result: Vec<Parsed> = vec![];
        let text = pair.as_str().to_string();
        let rule = pair.to_string();
        // A non-terminal pair can be converted to an iterator of the tokens which make it up:
        pinshot_recurse(pair.into_inner(), &mut result);

        let item = Parsed {
            rule: rule,
            text: text,
            children: result,
        };
        items.push(item);
    }
}
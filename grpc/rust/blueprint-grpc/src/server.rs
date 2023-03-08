use std::collections::HashMap;
//////////////
// Pin-Shot //
//////////////
extern crate pest;
#[macro_use]
extern crate pest_derive;

use std::any::Any;
use pest::Parser;
use pest::iterators::Pairs;

#[derive(Parser)]
#[grammar = "avx-quelle.pest"]
struct QuelleParser;

struct Parsed {
    rule: String,
    text: String,
    children: Vec<Parsed>,
}

struct RootParse {
    input: String,
    result: Vec<Parsed>,
    error: String,
}

///////////////
// Blueprint //
///////////////
use blueprint::blueprint_blue_server::BlueprintBlue;
use tonic::{transport::Server, Request, Response, Status};
use crate::blueprint::{ParseRequest, ParseResponse, QStatement, QExplicitCommand, QHelp, q_statement, q_explicit_command, QImplicitCommands};
use crate::blueprint::blueprint_blue_server::BlueprintBlueServer;

mod blueprint;

#[derive(Debug, Default)]
pub struct BlueprintBlueService {}

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

        let task = QuelleParser::parse(Rule::statement, &stmt);

        if task.is_ok() {
            let pairs = task.unwrap();
            pinshot_recurse(pairs, &mut pinshot.result);
        } else if task.is_err() {
            pinshot.error = task.unwrap_err().to_string();
        } else {
            pinshot.error = "internal error".to_string();
        }

        let mut result = Ok(Response::new(ParseResponse {
            input: String::from("foo"),
            output: None,
            error_lines: vec![],
        }),
        );
        if !pinshot.error.is_empty() {
            let lines = pinshot.error.split("\n");
            /*for mut line in lines {
                if Some(line) {
                    result.unwrap().into_inner().error_lines.append(Ok(line).to_string());
                }
            }*/
        } else {
            result.unwrap().into_inner().output = pinshot_extract_statement(&pinshot.result);
        }
        result
    }
}
/*
                directives: Some(q_statement::Directives::Explicit(QExplicitCommand {
                    text: String::from("foo"),
                    verb: String::from("@Help"),
                    topic: String::from("help"),
                    command: Some(q_explicit_command::Command::Help(QHelp {
                        topic: String::from("bar"),
                    })),
                })),
 */

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

fn pinshot_extract_statement(items: &Vec<Parsed>) -> Option<QStatement>
{
    let mut i: u8 = 0;
    let mut stmt: Option<QStatement> = None;
    for candidate in items {
        i = i + 1;
        if i == 1 {
            let mut is_explicit = candidate.children.len() == 1;
            if is_explicit {
                for child in candidate.children {
                    is_explicit = candidate.rule.starts_with("@");
                    break;
                }
            }
            if candidate.rule == "statement" {
                stmt = Some(QStatement {
                    text: candidate.text.clone(),
                    is_valid: true,
                    is_explicit: is_explicit,
                    message: "".to_string(),
                    directives: None,
                });
                if !candidate.children.is_empty() {
                    let children = &candidate.children;
                    if is_explicit {
                        stmt.unwrap().directives = Some(q_statement::Directives::Explicit(pinshot_extract_explcit_command(&children).unwrap()));
                    }
                    else if candidate.children.len() >= 1 {
                        stmt.unwrap().directives = Some(q_statement::Directives::Implicit(pinshot_extract_implicit_commands(&children).unwrap()));
                    }
                }
            }
        }
        else {
            return None;
        }
    }
    stmt
}

fn pinshot_extract_implicit_commands(items: &Vec<Parsed>) -> Option<QImplicitCommands>
{
    None
}

fn pinshot_extract_explcit_command(items: &Vec<Parsed>) -> Option<QExplicitCommand>
{
    None
}
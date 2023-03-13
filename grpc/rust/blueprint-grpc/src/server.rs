use std::collections::HashMap;
//////////////
// Pin-Shot //
//////////////
extern crate pest;
#[macro_use]
extern crate pest_derive;

use std::any::Any;
use std::borrow::Borrow;
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
use crate::blueprint::{ParseRequest, ParseResponse, QStatement, QImplicitCommand, QExplicitCommand, QHelp, q_statement, q_explicit_command, QImplicitCommands, QExit, QVersion, QReview, QSet, QGet, QExpand, QDelete, QFind, q_implicit_command, QFilter, QInvoke, QExec, QExport, QDisplay, QMacro, QClear};
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
            input: req.statement_text.clone(),
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

        let mut result: ParseResponse = ParseResponse {
            input: req.statement_text.clone(),
            output: None,
            error_lines: vec![],
        };
        if pinshot.error.is_empty() {
            result.output = pinshot_extract_statement(&pinshot.result);
        }

        if !pinshot.error.is_empty() {
            let lines = pinshot.error.split("\n");
            let mut i = 0;
            for mut line in lines {
                if !line.is_empty() {
                    result.error_lines.insert(i, line.to_string());
                    i = i + 1;
                }
            }
        }
        return Ok(Response::new(result));
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

fn pinshot_extract_statement(items: &Vec<Parsed>) -> Option<QStatement>
{
    for i in 0 .. items.len() {
        let candidate = items[i].borrow();
        if i == 1 {
            let mut is_explicit = candidate.children.len() == 1;
            if is_explicit {
                is_explicit = candidate.children[0].rule.starts_with("@");
            }
            if candidate.rule == "statement" {
                let mut stmt = QStatement {
                    text: candidate.text.clone(),
                    is_valid: true,
                    is_explicit: is_explicit,
                    message: "".to_string(),
                    directives: None,
                };
                if !candidate.children.is_empty() {
                    let children = &candidate.children;
                    if is_explicit {
                        stmt.directives = Some(q_statement::Directives::Explicit(pinshot_extract_explcit_command(&children).unwrap()));
                    }
                    else if candidate.children.len() >= 1 {
                        stmt.directives = Some(q_statement::Directives::Implicit(pinshot_extract_implicit_commands(&children).unwrap()));
                    }
                }
                return Some(stmt);
            }
        }
    }
    None
}

fn pinshot_extract_implicit_commands(items: &Vec<Parsed>) -> Option<QImplicitCommands>
{
    let mut commands: Vec<QImplicitCommand> = vec![];

    let mut cnt = 0;
    for i in 0 .. items.len() {
        let implicit = items[i].borrow();

        let mut icommand = QImplicitCommand {
            text: implicit.text.clone(),
            verb: implicit.rule.clone(),
            topic: implicit.text.clone(),
            command: None,
        };
        if implicit.rule.eq_ignore_ascii_case("find") && implicit.children.len() >= 1 {
            icommand.command = Option::from(q_implicit_command::Command::Find(QFind {
                is_quoted: false,
                segments: vec![],
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("filter") && implicit.children.len() >= 1 {
            let mut scopes: Vec<String> = vec![];
            for f in 0 .. implicit.children.len() {
                scopes.insert(i, implicit.children[f].text.clone())
            }
            icommand.command = Option::from(q_implicit_command::Command::Filter(QFilter {
                scope: scopes,
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("set") && implicit.children.len() >= 2 {
            icommand.command = Option::from(q_implicit_command::Command::Set(QSet {
                key: implicit.children[0].text.clone(),
                value: implicit.children[1].text.clone(),
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("clear") && implicit.children.len() >= 1 {
            icommand.command = Option::from(q_implicit_command::Command::Clear(QClear {
                key: implicit.children[0].text.clone(),
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("macro") && implicit.children.len() >= 1 {
            icommand.command = Option::from(q_implicit_command::Command::Macro(QMacro {
                label: implicit.children[0].text.clone(),
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("export") && implicit.children.len() >= 1 {
            icommand.command = Option::from(q_implicit_command::Command::Export(QExport {
                pathspec: implicit.children[0].text.clone(),
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("display") && implicit.children.len() >= 1 {
            let mut fields: Vec<u32> = vec![];
            for f in 0 .. implicit.children.len() {
                let f = u32::from_str_radix(&implicit.children[f].text, 10);
                fields.insert(i, f.unwrap())
            }
            icommand.command = Option::from(q_implicit_command::Command::Display(QDisplay {
                fields,
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("exec") && implicit.children.len() >= 1 {
            icommand.command = Option::from(q_implicit_command::Command::Exec(QExec {
                command: u32::from_str_radix(&implicit.children[0].text, 10).unwrap(),
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        } else if implicit.rule.eq_ignore_ascii_case("invoke") && implicit.children.len() >= 1 {
            icommand.command = Option::from(q_implicit_command::Command::Invoke(QInvoke {
                label: implicit.children[0].text.clone(),
            }));
            commands.insert(cnt, icommand);
            cnt = cnt + 1;
        }
    }
    Some(QImplicitCommands { items: commands })
}

fn pinshot_extract_explcit_command(items: &Vec<Parsed>) -> Option<QExplicitCommand>
{
    if items.len() == 1 {
        let mut command = QExplicitCommand {
            text: items[0].text.clone(),
            verb: items[0].rule.clone(),
            topic: items[0].text.clone(),
            command: None,
        };
        if items[0].rule.eq_ignore_ascii_case("help") && items[0].children.len() >= 1 {
            command.command = Option::from(q_explicit_command::Command::Help(QHelp {
                topic: items[0].children[0].text.clone(),
            }));
        }
        else if items[0].rule.eq_ignore_ascii_case("exit") {
            let mut arguments: Vec<String> = vec![];
            for i in 0 .. items[0].children.len() {
                arguments.insert(i, items[0].children[i].text.clone())
            }
            let explicit = q_explicit_command::Command::Exit(QExit {
                args: arguments,
            });
            command.command = Option::from(explicit);
        }
        else if items[0].rule.eq_ignore_ascii_case("version") {
            let mut arguments: Vec<String> = vec![];
            for i in 0 .. items[0].children.len() {
                arguments.insert(i, items[0].children[i].text.clone())
            }
            let explicit = q_explicit_command::Command::Version(QVersion {
                args: arguments,
            });
            command.command = Option::from(explicit);
        }
        else if items[0].rule.eq_ignore_ascii_case("expand") {
            let explicit = q_explicit_command::Command::Expand(QExpand {
                label: if items[0].children.len() == 1 && items[0].children[0].rule.eq_ignore_ascii_case("label") { items[0].children[0].text.clone() } else { "".to_string() },
            });
            command.command = Option::from(explicit);
        }
        else if items[0].rule.eq_ignore_ascii_case("delete") {
            let explicit = q_explicit_command::Command::Delete(QDelete {
                label: if items[0].children.len() == 1 && items[0].children[0].rule.eq_ignore_ascii_case("label") { items[0].children[0].text.clone() } else { "".to_string() },
            });
            command.command = Option::from(explicit);
        }
        else if items[0].rule.eq_ignore_ascii_case("get") {
            let mut arguments: Vec<String> = vec![];
            for i in 0 .. items[0].children.len() {
                arguments.insert(i, items[0].children[i].text.clone())
            }
            let explicit = q_explicit_command::Command::Get(QGet {
                keys: arguments,
            });
            command.command = Option::from(explicit);
        }
        else {
            return None;
        }
        return Some(command);
    }
    None
}
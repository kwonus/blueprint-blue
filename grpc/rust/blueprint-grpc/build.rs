use std::env;
use std::path::PathBuf;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let proto_file = "../api/proto/blueprint.proto";
    let out_dir = PathBuf::from(env::var("../").unwrap());

    tonic_build::configure()
        .build_client(true)
        .build_server(true)
        .file_descriptor_set_path(out_dir.join("blueprint_descriptor.bin"))
        .out_dir("./src")
        .compile(&[proto_file], &["proto"])?;

    Ok(())
}
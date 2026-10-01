use github_copilot_sdk::session_events::AssistantMessageData;
use github_copilot_sdk::types::SessionConfig;
use github_copilot_sdk::{Client, ClientOptions};

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let prompt = "Este programa ya usa el crate oficial github-copilot-sdk 1.0.11. \
        Explica en una frase qué aporta GitHub Copilot SDK para programadores de Rust.";

    println!("Pregunta:");
    println!("{prompt}");
    println!();
    println!("Respuesta:");

    let client = Client::start(ClientOptions::default()).await?;
    let session = client.create_session(SessionConfig::default()).await?;
    let response = session.send_and_wait(prompt).await?;

    let content = response
        .as_ref()
        .and_then(|event| event.typed_data::<AssistantMessageData>())
        .map(|data| data.content)
        .unwrap_or_else(|| "(sin respuesta)".to_owned());
    println!("{content}");

    session.disconnect().await?;
    client.stop().await?;
    Ok(())
}

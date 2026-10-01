import { CopilotClient } from "@github/copilot-sdk";

const client = new CopilotClient();
await client.start();

try {
  const prompt =
    "Explica en una frase qué aporta GitHub Copilot SDK " +
    "para programadores de TypeScript.";
  console.log("Pregunta:");
  console.log(prompt);
  console.log();
  console.log("Respuesta:");

  const session = await client.createSession({});
  try {
    const response = await session.sendAndWait({
      prompt,
    });
    console.log(
      response?.data && "content" in response.data
        ? response.data.content
        : response,
    );
  } finally {
    await session.disconnect();
  }
} finally {
  await client.stop();
}

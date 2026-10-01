import { CopilotClient } from "@github/copilot-sdk";

const client = new CopilotClient();
await client.start();

try {
  const session = await client.createSession({});
  try {
    const response = await session.sendAndWait({
      prompt:
        "Explica en una frase qué aporta GitHub Copilot SDK " +
        "para programadores de TypeScript.",
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

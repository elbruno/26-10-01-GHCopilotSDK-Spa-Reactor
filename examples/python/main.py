import asyncio

from copilot import CopilotClient
from copilot.session_events import (
    AssistantMessageData,
    SessionErrorData,
    SessionIdleData,
)


async def main() -> None:
    prompt = (
        "Explica en una frase qué aporta GitHub Copilot SDK "
        "para programadores de Python."
    )
    print("Pregunta:")
    print(prompt)
    print()
    print("Respuesta:")

    async with CopilotClient() as client:
        async with await client.create_session() as session:
            done = asyncio.Event()
            error: RuntimeError | None = None

            def on_event(event) -> None:
                nonlocal error
                match event.data:
                    case AssistantMessageData(content=content):
                        print(content)
                    case SessionErrorData(message=message):
                        error = RuntimeError(message)
                        done.set()
                    case SessionIdleData():
                        done.set()

            session.on(on_event)
            await session.send(prompt)
            await done.wait()
            if error is not None:
                raise error


if __name__ == "__main__":
    asyncio.run(main())

package main

import (
	"context"
	"fmt"

	copilot "github.com/github/copilot-sdk/go"
)

func main() {
	client := copilot.NewClient(&copilot.ClientOptions{LogLevel: "error"})
	if err := client.Start(context.Background()); err != nil {
		panic(err)
	}
	defer client.Stop()

	session, err := client.CreateSession(context.Background(), &copilot.SessionConfig{})
	if err != nil {
		panic(err)
	}
	defer session.Disconnect()

	response, err := session.SendAndWait(context.Background(), copilot.MessageOptions{
		Prompt: "Explica en una frase qué aporta GitHub Copilot SDK.",
	})
	if err != nil {
		panic(err)
	}
	if response != nil {
		if message, ok := response.Data.(*copilot.AssistantMessageData); ok {
			fmt.Println(message.Content)
		}
	}
}

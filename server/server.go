package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"net"
)

type InputEvent struct {
	EventType string `json:"eventType"`
	Key       string `json:"key,omitempty"`
	X         int    `json:"x,omitempty"`
	Y         int    `json:"y,omitempty"`
}

func main() {
	port := "8080"

	listener, err := net.Listen("tcp", ":"+port)
	if err != nil {
		fmt.Println("[Server] Error starting server:", err)
		return
	}
	defer listener.Close()

	fmt.Println("[Server] Listening on port", port)

	for {
		conn, err := listener.Accept()
		if err != nil {
			fmt.Println("[Server] Error accepting connection:", err)
			continue
		}
		fmt.Println("[Server] Client connected:", conn.RemoteAddr())

		go handleConnection(conn)
	}
}

func handleConnection(conn net.Conn) {
	defer conn.Close()

	reader := bufio.NewReader(conn)

	for {
		message, err := reader.ReadString('\n')
		if err != nil {
			fmt.Println("[Server] Connection closed:", conn.RemoteAddr())
			break
		}

		var event InputEvent
		err = json.Unmarshal([]byte(message), &event)
		if err != nil {
			fmt.Println("[Server] Error parsing event:", err)
			continue
		}

		switch event.EventType {
		case "mouse":
			fmt.Printf("[Server] Mouse moved to (%d, %d)\n", event.X, event.Y)
		case "keyboard":
			fmt.Printf("[Server] Key pressed: %s\n", event.Key)
		}

		// Send ACK to client
		conn.Write([]byte("ACK\n"))
	}
}

package main

import (
	"bufio"
	"fmt"
	"net"
	"os"
)

type InputEvent struct {
	EventType string `json:"eventType"` // "mouse" or "keyboard"
	Key       string `json:"key,omitempty"`
	X         int    `json:"x,omitempty"`
	Y         int    `json:"y,omitempty"`
}

func main() {
	serverIP := "192.168.1.210"
	port := "8080"

	conn, err := net.Dial("tcp", serverIP+":"+port)
	if err != nil {
		fmt.Println("[Client] Error connecting to server:", err)
		return
	}
	defer conn.Close()

	fmt.Println("[Client] Connected to server", serverIP)

	reader := bufio.NewReader(os.Stdin)
	for {
		fmt.Print("[Client] Enter message: ")
		text, _ := reader.ReadString('\n')

		_, err := fmt.Fprintf(conn, text+"\n")
		if err != nil {
			fmt.Println("[Client] Error sending message:", err)
			break
		}
	}
}

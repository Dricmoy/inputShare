package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"net"
	"time"

	"github.com/go-vgo/robotgo"
)

type InputEvent struct {
	EventType string `json:"eventType"`
	Key       string `json:"key,omitempty"`
	X         int    `json:"x,omitempty"`
	Y         int    `json:"y,omitempty"`
}

func main() {
	serverIP := "192.168.1.210" // Change to your server IP
	port := "8080"

	conn, err := net.Dial("tcp", serverIP+":"+port)
	if err != nil {
		fmt.Println("[Client] Error connecting to server:", err)
		return
	}
	defer conn.Close()

	fmt.Println("[Client] Connected to server", serverIP)

	reader := bufio.NewReader(conn)

	for {
		// Capture mouse position
		x, y := robotgo.GetMousePos()
		sendEvent(conn, InputEvent{
			EventType: "mouse",
			X:         x,
			Y:         y,
		})
		readACK(reader)

		// Capture example key presses
		keys := []string{"a", "s", "d", "w"}
		for _, key := range keys {
			if err := robotgo.KeyTap(key); err == nil {
				sendEvent(conn, InputEvent{
					EventType: "keyboard",
					Key:       key,
				})
				readACK(reader)
			} else {
				fmt.Printf("[Client] Error tapping key %s: %v\n", key, err)
			}
		}

		time.Sleep(50 * time.Millisecond) // ~20 updates/sec
	}
}

func sendEvent(conn net.Conn, event InputEvent) {
	data, err := json.Marshal(event)
	if err != nil {
		fmt.Println("[Client] Error marshaling event:", err)
		return
	}
	_, err = conn.Write(append(data, '\n'))
	if err != nil {
		fmt.Println("[Client] Error sending event:", err)
	}
}

func readACK(reader *bufio.Reader) {
	ack, err := reader.ReadString('\n')
	if err != nil {
		fmt.Println("[Client] Error reading ACK:", err)
		return
	}
	fmt.Printf("[Client] Server response: %s", ack)
}

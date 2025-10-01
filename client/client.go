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
	serverIP := "192.168.1.210"
	port := "8080"

	conn, err := net.Dial("tcp", serverIP+":"+port)
	if err != nil {
		fmt.Println("[Client] Error connecting to server:", err)
		return
	}
	defer conn.Close()

	fmt.Println("[Client] Connected to server", serverIP)
	reader := bufio.NewReader(conn)

	lastX, lastY := -1, -1
	keys := []string{"a", "s", "d", "w"} // Example keys to monitor

	for {
		// Mouse position tracking
		x, y := robotgo.GetMousePos()

		if x != lastX || y != lastY {
			sendEvent(conn, InputEvent{
				EventType: "mouse",
				X:         x,
				Y:         y,
			})
			readACK(reader)
			lastX, lastY = x, y
		}

		// Key press tracking (polling method)
		for _, key := range keys {
			err := robotgo.KeyTap(key)
			if err == nil {
				sendEvent(conn, InputEvent{
					EventType: "keyboard",
					Key:       key,
				})
				readACK(reader)
			}
		}

		time.Sleep(10 * time.Millisecond) // ~100 checks/sec
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

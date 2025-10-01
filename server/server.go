package main

import (
	"bufio"
	"fmt"
	"net"
)

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
		fmt.Printf("[Server] Received: %s", message)
	}
}

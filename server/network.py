import json
import socket
from typing import Optional, Tuple


class ServerNetwork:
    def __init__(self, host: str = "", port: int = 5050) -> None:
        self.host: str = host
        self.port: int = port

        self.conn: Optional[socket.socket] = None
        self.addr: Optional[Tuple[str, int]] = None

        self.sock: socket.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.bind((self.host, self.port))
        self.sock.listen(1)

        print(f"Server listening on port {self.port}...")

    def accept_connection(self) -> None:
        self.conn, self.addr = self.sock.accept()
        print(f"Connected by {self.addr}")

    def send_event(self, event: dict) -> None:
        if self.conn:
            try:
                message = json.dumps(event) + "\n"
                self.conn.sendall(message.encode())
            except Exception as e:
                print(f"Send error: {e}")

    def close(self) -> None:
        if self.conn:
            self.conn.close()
        self.sock.close()

import socket
import json
from pynput.mouse import Controller

mouse = Controller()

HOST = '0.0.0.0'
PORT = 5050

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((HOST, PORT))
server.listen(1)

print("[*] Waiting for connection")
conn, addr = server.accept()
print(f"[+] Connected by {addr}")

while True:
    data = conn.recv(1024)
    if not data:
        break

    coords = json.loads(data.decode('utf-8'))
    mouse.position = (coords['x'] - 1000, coords['y'])

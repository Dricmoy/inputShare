from pynput import mouse
import socket, json, pyautogui

# The host and open port needs to have updated logic
HOST = '' 
PORT = 5050

screen_width, screen_height = pyautogui.size()
client =  socket.socket(socket.AF_INET, socket.SOCK_STREAM)i
client.connect(socket, port)

controller = mouse.Controller()

"""
Takes in a tuple of x and y coord of the mouse cursor and sends it to the reciever
"""
def send_cursor_position(pos : tuple):
    data = json.dumps({'x':pos[0], 'y':pos[1]})
    client.sendall(data.encode('utf-8'))

"""
Listener for cursor going across one screen
"""
def on_cursor_move(x : float, y: float):
    if x >= screen_width - 1:
        send_position((x, y))

with mouse.listener(on_cursor_move=on_cursor_move) as listener:
    listener.join()



import threading
from typing import Any, Callable, Dict

import pyautogui
from pynput import keyboard, mouse

SCREEN_WIDTH, SCREEN_HEIGHT = pyautogui.size()
EDGE_THRESHOLD = 10  # pixels near edge to trigger control switch


class InputHandler:
    def __init__(self, send_event_callback: Callable[[Dict[str, Any]], None]) -> None:
        """send_event_callback: function to call with serialized event data"""
        self.send_event: Callable[[Dict[str, Any]], None] = send_event_callback
        self.control_switched: bool = False
        self.lock: threading.Lock = threading.Lock()

    def check_edge(self, x: int, y: int) -> bool:
        # Example: switch control when mouse at right edge
        return x >= SCREEN_WIDTH - EDGE_THRESHOLD

    def on_move(self, x: int, y: int) -> None:
        with self.lock:
            if self.check_edge(x, y):
                if not self.control_switched:
                    print("Edge reached: Switching control to client.")
                    self.control_switched = True
            else:
                if self.control_switched:
                    print("Mouse moved back inside: Switching control back to host.")
                    self.control_switched = False

            if self.control_switched:
                event: Dict[str, Any] = {"type": "move", "x": x, "y": y}
                self.send_event(event)

    def on_click(self, x: int, y: int, button: mouse.Button, pressed: bool) -> None:
        with self.lock:
            if self.control_switched:
                event: Dict[str, Any] = {
                    "type": "click",
                    "x": x,
                    "y": y,
                    "button": str(button),
                    "pressed": pressed,
                }
                self.send_event(event)

    def on_scroll(self, x: int, y: int, dx: int, dy: int) -> None:
        with self.lock:
            if self.control_switched:
                event: Dict[str, Any] = {
                    "type": "scroll",
                    "x": x,
                    "y": y,
                    "dx": dx,
                    "dy": dy,
                }
                self.send_event(event)

    def on_press(self, key: keyboard.Key | keyboard.KeyCode) -> None:
        with self.lock:
            if self.control_switched:
                try:
                    k = key.char  # type: ignore
                except AttributeError:
                    k = str(key)
                event: Dict[str, Any] = {"type": "key", "action": "press", "key": k}
                self.send_event(event)

    def on_release(self, key: keyboard.Key | keyboard.KeyCode) -> None:
        with self.lock:
            if self.control_switched:
                try:
                    k = key.char  # type: ignore
                except AttributeError:
                    k = str(key)
                event: Dict[str, Any] = {"type": "key", "action": "release", "key": k}
                self.send_event(event)

    def start(self) -> None:
        self.mouse_listener = mouse.Listener(
            on_move=self.on_move, on_click=self.on_click, on_scroll=self.on_scroll
        )
        self.keyboard_listener = keyboard.Listener(
            on_press=self.on_press, on_release=self.on_release
        )

        self.mouse_listener.start()
        self.keyboard_listener.start()

    def join(self) -> None:
        self.mouse_listener.join()
        self.keyboard_listener.join()

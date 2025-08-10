from input_handler import InputHandler
from network import ServerNetwork


def main() -> None:
    network = ServerNetwork()
    network.accept_connection()

    # Pass send_event function to input handler
    input_handler = InputHandler(send_event_callback=network.send_event)
    input_handler.start()

    # Block main thread to keep listeners running
    input_handler.join()

    network.close()


if __name__ == "__main__":
    main()

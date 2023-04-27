import socket
import struct
from io import BytesIO
from mss import mss
from PIL import Image

def capture_screen():
    with mss() as sct:
        monitor = sct.monitors[0]
        screenshot = sct.grab(monitor)
        img = Image.frombytes("RGB", screenshot.size, screenshot.bgra, "raw", "BGRX")
        return img

def main():
    server_ip = "0.0.0.0"
    server_port = 9999

    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((server_ip, server_port))
    server_socket.listen(1)
    print(f"Server started on {server_ip}:{server_port}...")

    client_socket, client_address = server_socket.accept()
    print(f"Client connected from {client_address}!")

    try:
        while True:
            img = capture_screen()
            with BytesIO() as buf:
                img.save(buf, format="JPEG")
                img_data = buf.getvalue()

                # Send the image length and data
                img_length = len(img_data)
		# client_socket.sendall(struct.pack(">I", img_length))
                print(f'Sending image of size {img_length} bytes')
                client_socket.sendall(struct.pack(">I", img_length))
                client_socket.sendall(img_data)
    finally:
        client_socket.close()
        server_socket.close()

if __name__ == "__main__":
    main()

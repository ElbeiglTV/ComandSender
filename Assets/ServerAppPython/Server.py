# -*- coding: utf-8 -*-
import os
import socket
import threading
import time

# Configuración del servidor
HOST = '0.0.0.0'          # Escuchar en todas las interfaces
PORT = 65432              # Puerto de escucha
BROADCAST_PORT = 65433    # Puerto para broadcast
COMMANDS_FOLDER = "commands"  # Carpeta donde se encuentran los archivos de comandos

def broadcast_presence():
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as sock:
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
        message = b'SERVER_ALIVE'
        while True:
            sock.sendto(message, ('<broadcast>', BROADCAST_PORT))
            time.sleep(5)

def handle_client(client_socket):
    while True:
        try:
            command = client_socket.recv(1024).decode()
            if not command:
                break
            if command.lower() == 'exit':
                break
            if command == 'GET_COMMANDS':
                send_command_files(client_socket)
            else:
                # Ejecutar comando de consola, etc...
                pass
        except Exception as e:
            print(f"Error: {str(e)}")
            break
    client_socket.close()
    
def send_command_files(client_socket):
    command_files = os.listdir(COMMANDS_FOLDER)
    response = []
    for file_name in command_files:
        file_path = os.path.join(COMMANDS_FOLDER, file_name)
        with open(file_path, 'r') as file:
            file_content = file.read()
            response.append(f"{file_name}|{file_content}")
    
    response_message = "\n".join(response)
    client_socket.send(response_message.encode())

def main():
    if not os.path.exists(COMMANDS_FOLDER):
        os.makedirs(COMMANDS_FOLDER)

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind((HOST, PORT))
    server.listen(5)
    print(f"[*] Escuchando en {HOST}:{PORT}")

    broadcast_thread = threading.Thread(target=broadcast_presence, daemon=True)
    broadcast_thread.start()

    while True:
        client_socket, addr = server.accept()
        print("[*] Conexion aceptada de", addr)
        client_handler = threading.Thread(target=handle_client, args=(client_socket,))
        client_handler.start()

if __name__ == "__main__":
    main()

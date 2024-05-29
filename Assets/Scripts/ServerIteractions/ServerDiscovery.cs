using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;

public class ServerDiscovery : MonoBehaviour
{
    public TMP_Text discoveredServersText;
    public CommandSender commandSender;
    public CommandCatch commandCatch;
    public ButtonController buttonController;
    public GameObject serverButtonPrefab;
    public Transform contentParent;

    private const int broadcastPort = 65433;
    private UdpClient udpClient;
    private bool isListening;
    private HashSet<string> discoveredServers = new HashSet<string>();

    void Start()
    {
        udpClient = new UdpClient(broadcastPort);
        isListening = true;
        ListenForBroadcasts();
    }

    private async void ListenForBroadcasts()
    {
        while (isListening)
        {
            UdpReceiveResult result = await udpClient.ReceiveAsync();
            string message = Encoding.ASCII.GetString(result.Buffer);
            if (message == "SERVER_ALIVE")
            {
                string serverIP = result.RemoteEndPoint.Address.ToString();
                if (discoveredServers.Add(serverIP))
                {
                    CreateServerButton(serverIP);
                }
            }
        }
    }

    private void CreateServerButton(string serverIP)
    {
        GameObject newButton = Instantiate(serverButtonPrefab, contentParent);
        newButton.GetComponentInChildren<TMP_Text>().text = serverIP;
        newButton.GetComponent<Button>().onClick.AddListener(() => OnServerSelected(serverIP));
        buttonController.ServerButons.Add(newButton.GetComponent<Button>());
        buttonController.InitialiseButtons();
    }

    private void OnServerSelected(string serverIP)
    {
        commandSender.SetServerIP(serverIP);
        discoveredServersText.text = $"Selected server: {serverIP}";
        ConnectAndReceiveFiles(serverIP);
    }

    public async void ConnectAndReceiveFiles(string serverIP)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                Debug.Log("Connecting to server...");
                await client.ConnectAsync(serverIP, commandSender.serverPort);
                Debug.Log("Connected to server.");

                using (NetworkStream stream = client.GetStream())
                {
                    // Enviar comando para obtener archivos
                    byte[] request = Encoding.UTF8.GetBytes("GET_COMMANDS");
                    await stream.WriteAsync(request, 0, request.Length);
                    Debug.Log("Command sent, waiting for response...");

                    // Recibir archivos del servidor
                    await ReceiveFiles(stream);
                    commandCatch.ReadTextFiles();


                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception: {ex.Message}");
        }
    }

    private async Task ReceiveFiles(NetworkStream stream)
    {
        while (true)
        {
            // Leer la longitud del nombre del archivo
            byte[] lengthBuffer = new byte[4];
            int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead != 4) break;

            int fileNameLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (fileNameLength == 0) break; // No más archivos

            // Leer el nombre del archivo
            byte[] fileNameBuffer = new byte[fileNameLength];
            bytesRead = await stream.ReadAsync(fileNameBuffer, 0, fileNameBuffer.Length);
            if (bytesRead != fileNameLength) break;

            string fileName = Encoding.UTF8.GetString(fileNameBuffer);

            // Leer la longitud del contenido del archivo
            bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead != 4) break;

            int fileContentLength = BitConverter.ToInt32(lengthBuffer, 0);

            // Leer el contenido del archivo
            byte[] fileContentBuffer = new byte[fileContentLength];
            int totalBytesRead = 0;
            while (totalBytesRead < fileContentLength)
            {
                bytesRead = await stream.ReadAsync(fileContentBuffer, totalBytesRead, fileContentLength - totalBytesRead);
                if (bytesRead == 0) break;
                totalBytesRead += bytesRead;
            }

            if (totalBytesRead != fileContentLength) break;

            // Guardar el archivo
            SaveFile(fileName, fileContentBuffer);
        }
    }

    private void SaveFile(string fileName, byte[] fileContent)
    {
        if (!Directory.Exists(Application.dataPath + "/Commands" + "/" + commandSender.serverIP)) Directory.CreateDirectory(Application.dataPath + "/Commands" + "/" + commandSender.serverIP);
        string filePath = Path.Combine(Application.dataPath+"/Commands"+"/"+commandSender.serverIP, fileName);
        File.WriteAllBytes(filePath, fileContent);
        Debug.Log($"File saved: {filePath}");
    }

    private void OnDestroy()
    {
        isListening = false;
        udpClient.Close();
    }
}

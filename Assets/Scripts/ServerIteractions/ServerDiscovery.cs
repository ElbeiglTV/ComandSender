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
        SendCommandToServer("GET_COMMANDS");
    }

    private async void SendCommandToServer(string command)
    {
        using (TcpClient client = new TcpClient())
        {
            try
            {
                Debug.Log($"Connecting to server {commandSender.serverIP}:{commandSender.serverPort}...");
                client.ReceiveTimeout = 5000;  // Tiempo de espera para recibir datos
                client.SendTimeout = 5000;     // Tiempo de espera para enviar datos

                await client.ConnectAsync(commandSender.serverIP, commandSender.serverPort);
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(command);
                await stream.WriteAsync(data, 0, data.Length);

                Debug.Log("Command sent, waiting for response...");

                byte[] buffer = new byte[4096];
                int bytesRead;
                StringBuilder response = new StringBuilder();

                // Aumentar el tiempo de espera de lectura en el stream
                stream.ReadTimeout = 10000;  // 10 segundos de tiempo de espera

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    response.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                }

                Debug.Log("Response received: " + response.ToString());
                ProcessReceivedCommands(response.ToString());
            }
            catch (SocketException e)
            {
                Debug.LogError("SocketException: " + e);
            }
            catch (IOException e)
            {
                Debug.LogError("IOException: " + e);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception: " + e);
            }
        }
    }

    private void ProcessReceivedCommands(string response)
    {
        Debug.Log("Processing received commands...");
        string[] fileEntries = response.Split(new string[] { "\n" }, System.StringSplitOptions.None);

        foreach (string entry in fileEntries)
        {
            if (!string.IsNullOrWhiteSpace(entry))
            {
                string[] fileData = entry.Split(new char[] { '|' }, 2);
                if (fileData.Length == 2)
                {
                    string fileName = fileData[0];
                    string fileContent = fileData[1];

                    string path = Path.Combine(Application.persistentDataPath, fileName);
                    File.WriteAllText(path, fileContent);

                    Debug.Log($"File saved: {path}");
                }
                else
                {
                    Debug.LogWarning("Invalid file entry: " + entry);
                }
            }
        }
    }

    private void OnDestroy()
    {
        isListening = false;
        udpClient.Close();
    }
}

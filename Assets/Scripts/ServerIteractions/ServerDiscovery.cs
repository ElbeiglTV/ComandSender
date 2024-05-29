using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

public class ServerDiscovery : MonoBehaviour
{
    public TMP_Text discoveredServersText;
    public CommandSender commandSender;
    public GameObject serverButtonPrefab;
    public Transform contentParent;

    private const int broadcastPort = 65433;
    private UdpClient udpClient;
    private bool isListening;
    private HashSet<string> discoveredServers = new HashSet<string>();

    private const int bufferSize = 1024; // Tamaño del búfer para recibir datos
    private string savePath = "/CommandFiles"; // Directorio para guardar los archivos de comandos

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
            else
            {
                SaveCommandFile(message);
            }
        }
    }

    private void CreateServerButton(string serverIP)
    {
        GameObject newButton = Instantiate(serverButtonPrefab, contentParent);
        newButton.GetComponentInChildren<TMP_Text>().text = serverIP;
        newButton.GetComponent<Button>().onClick.AddListener(() => OnServerSelected(serverIP));
    }

    private void OnServerSelected(string serverIP)
    {
        commandSender.SetServerIP(serverIP);
        discoveredServersText.text = $"Selected server: {serverIP}";

        commandSender.SendCommandToServer("GET_COMMANDS");
    }

    private void SaveCommandFile(string fileData)
    {
        // Crear el directorio si no existe
        string directoryPath = Application.dataPath + savePath;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Generar un nombre único para el archivo
        string fileName = "command_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
        string filePath = Path.Combine(directoryPath, fileName);

        // Escribir los datos del archivo
        File.WriteAllText(filePath, fileData);
        Debug.Log("Archivo de comando guardado: " + filePath);
    }

    private void OnDestroy()
    {
        isListening = false;
        udpClient.Close();
    }
}

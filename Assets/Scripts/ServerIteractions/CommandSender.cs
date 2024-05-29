using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class CommandSender : MonoBehaviour
{
    public InputField commandInputField; // Campo de entrada de texto para ingresar el comando
    public Button sendButton; // Bot�n para enviar el comando
    public Text responseText; // Texto UI para mostrar la respuesta del servidor

    public string serverIP = "192.168.1.100"; // Direcci�n IP del servidor (valor por defecto)
    public int serverPort = 65432; // Puerto del servidor

    void Start()
    {
        sendButton.onClick.AddListener(SendCommand); // A�ade un listener para manejar el click del bot�n de enviar
    }

    public void SetServerIP(string ip)
    {
        serverIP = ip; // M�todo para establecer la IP del servidor
    }

    async void SendCommand()
    {
        string command = commandInputField.text; // Obtiene el comando ingresado en el campo de entrada de texto
        string response = await SendCommandToServer(command); // Env�a el comando al servidor y espera la respuesta
        responseText.text = response; // Muestra la respuesta del servidor en la UI
    }

    public async Task<string> SendCommandToServer(string command)
    {
        using (TcpClient client = new TcpClient())
        {
            try
            {
                await client.ConnectAsync(serverIP, serverPort); // Conecta con el servidor utilizando la IP y puerto especificados
                NetworkStream stream = client.GetStream(); // Obtiene el stream de la conexi�n
                byte[] data = Encoding.ASCII.GetBytes(command); // Codifica el comando en bytes
                await stream.WriteAsync(data, 0, data.Length); // Env�a el comando al servidor

                data = new byte[1024]; // Buffer para recibir la respuesta
                int bytes = await stream.ReadAsync(data, 0, data.Length); // Lee la respuesta del servidor
                return Encoding.ASCII.GetString(data, 0, bytes); // Decodifica y devuelve la respuesta del servidor
            }
            catch (SocketException e)
            {
                Debug.LogError("SocketException: " + e); // Registra cualquier excepci�n de socket
                return "Error: " + e.Message; // Devuelve un mensaje de error
            }
        }
    }
}

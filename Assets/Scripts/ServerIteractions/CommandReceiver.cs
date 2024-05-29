using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class CommandReceiver : MonoBehaviour
{
    public CommandSender commandSender;
    public string saveDirectory = "ReceivedCommands"; // Carpeta donde se guardarán los archivos recibidos

    private TcpClient client;

    void Start()
    {
        Task.Run(() => ReceiveFiles());
    }

    async Task ReceiveFiles()
    {
        client = new TcpClient();
        try
        {
            await client.ConnectAsync(commandSender.serverIP, commandSender.serverPort);
            NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                string filePath = Path.Combine(saveDirectory, "received_file.txt");
                using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                {
                    await fs.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e);
        }
        finally
        {
            client.Close();
        }
    }
}

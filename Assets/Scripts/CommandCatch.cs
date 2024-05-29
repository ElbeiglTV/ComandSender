using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CommandCatch : MonoBehaviour
{
    public string folderPath; // Ruta de la carpeta que contiene los archivos TXT
    public List<CommandData> textFilesData = new List<CommandData>();
    public CommandButtonConfigurator CommandButtonPrefab;
    public Transform CommandButtonParent;
    [ContextMenu("ReloadCommands")]
    public void ReadTextFiles()
    {
        // Verifica que la carpeta exista
        if (!Directory.Exists(Application.dataPath + "/Commands"))
        {
            Debug.LogError("La carpeta especificada no existe.  " + Application.dataPath + "/Commands");
            return;
        }

        // Obtiene la lista de archivos TXT en la carpeta
        string[] txtFiles = Directory.GetFiles(Application.dataPath+"/Commands", "*.txt");

        // Lee cada archivo TXT y guarda su nombre y contenido en la lista
        foreach (string filePath in txtFiles)
        {
            CommandData fileData = new CommandData();
            fileData.fileName = Path.GetFileName(filePath);
            fileData.fileContent = File.ReadAllText(filePath);
            textFilesData.Add(fileData);
            CommandButtonConfigurator CommandButton = Instantiate(CommandButtonPrefab, CommandButtonParent);
            CommandButton.InitializeButton(fileData);
        }

        // Ahora la lista 'textFilesData' contiene la información de todos los archivos TXT en la carpeta
        // Puedes acceder a ellos a través de índices o iterar sobre la lista según sea necesario
    }
}
public struct CommandData
{
    public string fileName;
    public string fileContent;
}

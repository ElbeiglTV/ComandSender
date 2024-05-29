using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonConfigurator : MonoBehaviour
{
    private CommandData _myCommandData;
    public Button myButon;
    public CommandSender mySender;
    public TextMeshProUGUI myTmpro;
    public void InitializeButton(CommandData commandData,CommandSender commandSender)
    {
        _myCommandData = commandData;
        myTmpro.text = commandData.fileName;
    }
    public void SendCommand()
    {
        mySender.SendCommand(_myCommandData.fileContent);
    }

}

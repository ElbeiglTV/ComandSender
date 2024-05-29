using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonConfigurator : MonoBehaviour
{
    private CommandData _myCommandData;
    public Button myButon;
    public TextMeshProUGUI myTmpro;
    public void InitializeButton(CommandData commandData)
    {
        _myCommandData = commandData;
        myTmpro.text = commandData.fileName;
    }
}

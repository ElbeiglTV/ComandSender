using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public List<Button> ServerButons = new();
    public NoServerButtonComands NoServerButtonComands;
    public void InitialiseButtons()
    {
        if (ServerButons.Count == 0) return;
        foreach (Button button in ServerButons)
        {
          button.onClick.AddListener(() => NoServerButtonComands.CloseServerList());
        }
       
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoServerButtonComands : MonoBehaviour
{
   public Animator ServerSelectorAnimator;
    public void OpenServerList()
    {
        ServerSelectorAnimator.SetBool("Open", true);
    }
    public void CloseServerList()
    {
        ServerSelectorAnimator.SetBool("Open", false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketListen : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] XRSocketInteractor keySocket;
    [SerializeField] bool isLocked;

    void Start()
    {
        if(keySocket !=null)
        { keySocket.selectEntered.AddListener(onSocketFunction); }
    }

    // Update is called once per frame
   private void onSocketFunction(SelectEnterEventArgs arg0)
    {
        isLocked = false;
        Debug.Log("UNLOCKED!!!!!!!!!");
    }
}

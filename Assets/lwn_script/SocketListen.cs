using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketListen : MonoBehaviour
{
    [SerializeField] XRSocketInteractor keySocket;
    [SerializeField] bool isLocked = true;

    [Header("Scripts to disable when locked")]
    [SerializeField] MonoBehaviour script1;
    [SerializeField] MonoBehaviour script2;

    void Start()
    {
        if (keySocket != null)
        {
            keySocket.selectEntered.AddListener(OnKeyInserted);
            keySocket.selectExited.AddListener(OnKeyRemoved);   // ¡û ÐÂÔö
        }

        ApplyLockState();
    }

    private void OnKeyInserted(SelectEnterEventArgs arg0)
    {
        isLocked = false;
        ApplyLockState();
        Debug.Log("UNLOCKED!!");
    }

    private void OnKeyRemoved(SelectExitEventArgs arg0)
    {
        isLocked = true;        // ¡û »Ö¸´Ëø¶¨
        ApplyLockState();
        Debug.Log("LOCKED!!");
    }

    private void ApplyLockState()
    {
        if (script1 != null)
            script1.enabled = !isLocked;

        if (script2 != null)
            script2.enabled = !isLocked;
    }
}

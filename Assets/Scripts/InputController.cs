using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    // Start is called before the first frame update
    public InputActionReference TestA;
    void Start()
    {
        TestA.action.performed += TestAFunction;
    }
    private void OnDestroy()
    {
        TestA.action.performed -= TestAFunction;
    }

    private void TestAFunction(InputAction.CallbackContext context)
    {
        Debug.Log("TestA");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

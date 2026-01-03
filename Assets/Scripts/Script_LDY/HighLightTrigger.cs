using UnityEngine;

public class SimpleHighlight : MonoBehaviour
{
    public float checkRange = 2.0f;

    private Outline myOutline;
    private Transform playerHead;

    void Start()
    {
        myOutline = GetComponent<Outline>();

        if (Camera.main != null)
        {
            playerHead = Camera.main.transform;
        }

        if (myOutline != null) myOutline.enabled = false;
    }

    void Update()
    {
        if (playerHead == null || myOutline == null) return;

        float distance = Vector3.Distance(transform.position, playerHead.position);

        if (distance <= checkRange)
        {
            myOutline.enabled = true;
        }
        else
        {
            myOutline.enabled = false;
        }
    }
}
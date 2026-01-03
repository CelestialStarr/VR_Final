using UnityEngine;

public class SimpleForward : MonoBehaviour
{
    public float speed = 3.0f; 

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtoAniControl : MonoBehaviour
{
    public Animator anim;

    public void SetStop()
    {
        anim.SetBool("Arrived", true);
    }
    public void LoadTownScene()
    {
        SceneManager.LoadScene("GameCafe");
    }
}

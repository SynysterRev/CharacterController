using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    [SerializeField] Camera TPSCamera;
    [SerializeField] Material mat;

    bool needToUpdate;
    float dissolve;
    // Start is called before the first frame update
    void Start()
    {
        dissolve = 1.0f;
        mat.SetFloat("_DissolveAmount", dissolve);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (needToUpdate)
        {
            MakeMobAppear();
        }
        
    }

    //disable tps camera and enable world boss cam
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TPSCamera.GetComponent<ThirdPersonCamera>().enabled = false;
            TPSCamera.GetComponent<WorldBossCamera>().enabled = true;
            needToUpdate = true;
        }
    }

    //use dissolve shader to make the zombie appear
    void MakeMobAppear()
    {
            mat.SetFloat("_DissolveAmount", dissolve);
            dissolve -= Time.deltaTime * 0.2f;
        if (dissolve <= 0.0f)
            needToUpdate = false;
    }
}

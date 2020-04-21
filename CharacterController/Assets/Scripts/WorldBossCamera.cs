using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBossCamera : MonoBehaviour
{
    [SerializeField] Transform target;

    [SerializeField] Transform player;

    [SerializeField] Vector3 offSet;

    [SerializeField] float XrotationSpeed = 70.0f;

    [SerializeField] float damping = 10.0f;

    [SerializeField] float speed = 10.0f;

    Vector3 destination;

    Vector3 targetPos;

    float rotX = 0.0f;

    public Transform Target
    {
        get => target;
    }


    // Start is called before the first frame update
    void Start()
    {
        targetPos = target.position + Vector3.up * 4.0f;
    }

    // Update is called once per frame
    void Update()
    {
        RotateAroundBoss();
    }
    
    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, destination, speed);
        LookTarget();
    }

    void LookTarget()
    {
        Quaternion rotation = Quaternion.LookRotation(targetPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, damping);
    }

    void RotateAroundBoss()
    {
        //calculate direction between center and player and normalized it
        Vector3 dir = (target.position - player.position).normalized;
        //give next position and stay at the right distance of player
        destination = target.position + dir * offSet.z;
        destination.y = offSet.y;
    }
}

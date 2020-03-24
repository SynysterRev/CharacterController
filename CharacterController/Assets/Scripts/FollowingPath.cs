using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingPath : MonoBehaviour
{
    [SerializeField] List<Transform> pointsList;

    GameObject player;
    Rigidbody rb;
    bool needToUpdate;
    bool hasBeenActivated;
    // Start is called before the first frame update
    void Start()
    {
        hasBeenActivated = false;
        needToUpdate = false;
        foreach (Transform transform in pointsList)
        {
            Vector3 pos = transform.position;
            pos.y = 0.0f;
            transform.position = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (needToUpdate)
            MoveToNextPoint();
    }

    //check if player reach a point of the path
    //if so delete this point et check next
    //do it until there are no points left
    void MoveToNextPoint()
    {
        if (pointsList.Count != 0)
        {
            Vector3 direction = pointsList[0].position - player.transform.position;
            rb.velocity = direction.normalized * 8.0f;
            if (Vector3.Distance(pointsList[0].position, player.transform.position) < 0.5f)
                pointsList.RemoveAt(0);
        }
        else
        {
            player.GetComponent<ThirdPersonController>().isInAutoPilot = false;
            player.GetComponent<ThirdPersonController>().enabled = false;
            player.GetComponent<ThirdPersonWBController>().enabled = true;
            needToUpdate = false;
        }
    }

    //player will move automatically following a path
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenActivated)
        {
            player = other.gameObject;
            player.GetComponent<ThirdPersonController>().isInAutoPilot = true;
            rb = player.GetComponent<Rigidbody>();
            hasBeenActivated = true;
            needToUpdate = true;
        }
    }
}

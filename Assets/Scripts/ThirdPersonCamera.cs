using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform target;

    [SerializeField]
    Vector3 offSet;

    Vector3 destination;
    Vector3 targetPos;

    float rotX = 0.0f;
    float rotY = 0.0f;

    [SerializeField]
    float damping = 10.0f;

    [SerializeField]
    float XrotationSpeed = 70.0f;

    [SerializeField]
    float YrotationSpeed = 70.0f;

    [SerializeField]
    float speed = 10.0f;

    [SerializeField]
    bool ReverseXAxis;

    [SerializeField]
    bool ReverseYAxis;

    float correctedOffsetZ;

    int mask;

    bool isReseting = false;

    void Start()
    {

        if (target == null)
        {
            if (GameObject.FindGameObjectWithTag("Player").transform == null)
                Debug.Log("error can't find player");
            else
                target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        mask = 1 << 8;
        mask = ~mask;
    }

    void Update()
    {
        if (Input.GetButtonDown("ResetCamera") && !isReseting)
        {
            StartCoroutine(ResetCam());
        }
        CalculateNewPosition();
        if (!isReseting)
            RotateAroundPlayer();
    }

    private void FixedUpdate()
    {
        FollowPlayer();
    }

    private void LateUpdate()
    {
        LookTarget();
    }

    //keep focus on the player
    void LookTarget()
    {
        Quaternion rotation = Quaternion.LookRotation(targetPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, damping * Time.deltaTime);
    }

    void CalculateNewPosition()
    {
        targetPos = target.position + Vector3.up * offSet.y;
        TestColliding(ref correctedOffsetZ, ref speed);

        //multiply current rotation by forward (get direction) and by offset to keep camera at the right distance
        destination = Quaternion.Euler(rotY, rotX, 0.0f) * Vector3.forward * correctedOffsetZ + targetPos;
    }

    void FollowPlayer()
    {
        transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * speed);
    }

    void RotateAroundPlayer()
    {
        rotX += Input.GetAxis("HorizontalCamera") * XrotationSpeed * Time.deltaTime * (ReverseXAxis ? -1 : 1);

        rotY += Input.GetAxis("VerticalCamera") * YrotationSpeed * Time.deltaTime * (ReverseYAxis ? -1 : 1);
        rotY = Mathf.Clamp(rotY, -55.0f, 70.0f);
    }

    //sphereraycast, if collision adjust distance to stay close to the wall and not inside
    void TestColliding(ref float correctedZ, ref float speed)
    {
        RaycastHit raycastHit;
        Vector3 dirToCam = (transform.position - targetPos).normalized;
        Debug.DrawRay(targetPos, dirToCam * -offSet.z, Color.yellow);


        if (Physics.SphereCast(targetPos, 0.4f, dirToCam, out raycastHit, -offSet.z, mask))
        {
            //teleport camera instead of lerping inside wall
            if (raycastHit.distance < -offSet.z)
            {
                transform.position = raycastHit.point + raycastHit.normal * 0.4f;
            }
            else
            {
                //correct distance between player and camera
                correctedZ = -raycastHit.distance;
            }
        }
        else
        {
            correctedZ = offSet.z;
        }

    }

    //see function name
    IEnumerator ResetCam()
    {
        isReseting = true;
        float cameraTargetAngle = -Mathf.DeltaAngle(rotX, target.eulerAngles.y);
        float cameraTargetAngleX = -Mathf.DeltaAngle(rotY, target.eulerAngles.x);
        while (Mathf.Abs(cameraTargetAngle) > 5.0f || Mathf.Abs(cameraTargetAngleX) > 1.0f)
        {
            if (Mathf.Abs(cameraTargetAngle) > 5.0f)
            {
                cameraTargetAngle = -Mathf.DeltaAngle(rotX, target.eulerAngles.y);
                rotX -= Mathf.Sign(cameraTargetAngle) * XrotationSpeed * Time.deltaTime;
            }

            if (Mathf.Abs(cameraTargetAngleX) > 1.0f)
            {
                cameraTargetAngleX = -Mathf.DeltaAngle(rotY, target.eulerAngles.x);
                rotY -= Mathf.Sign(cameraTargetAngleX) * YrotationSpeed * 0.8f * Time.deltaTime;
            }
            yield return null;
        }

        rotX = target.eulerAngles.y;
        rotY = target.eulerAngles.x;
        isReseting = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}

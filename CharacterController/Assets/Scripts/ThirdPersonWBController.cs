using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonWBController : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;

    [SerializeField] float speed = 0.0f;

    [SerializeField] float runSpeed;

    [SerializeField] float rotationSpeed;

    bool jumpInput;
    float horizontalInput;

    Vector3 destination;

    float rotX = 0.0f;

    Vector3 velocity;

    Transform targetMainCamera;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("missing rigidbody");
        }
        animator = GetComponent<Animator>();
        targetMainCamera = Camera.main.GetComponent<WorldBossCamera>().Target;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Movement();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal") * -1.0f;
    }

    private void LateUpdate()
    {
        SetAnimation();
    }

    void Movement()
    {
        //calculate rightVector of player from boss position to his position
        Vector3 rightVector = targetMainCamera.position - transform.position;

        //then its forward, so he can walk around the boss
        Vector3 forwardVector = Vector3.Cross(Vector3.up, rightVector.normalized);
        Debug.DrawRay(transform.position + Vector3.up * 1.0f, forwardVector * 3.0f, Color.white);
        if (horizontalInput != 0.0f)
        {
            forwardVector *= -horizontalInput;
            RotatePlayer(forwardVector);
            velocity = forwardVector.normalized * speed;
        }
        else
        {
            velocity = Vector3.zero;
        }

        //add a small velocity towards boss to readjust player direction
        Debug.DrawRay(transform.position + Vector3.up * 1.0f, rightVector.normalized * 2.0f, Color.black);
        if (Vector3.Distance(transform.position, targetMainCamera.position) > 11.5f)
        {
            velocity += rightVector.normalized * 0.5f;
        }
        velocity = Vector3.ClampMagnitude(velocity, speed);
        rb.velocity = velocity;
    }

    void SetAnimation()
    {
        animator.SetFloat("velocity", rb.velocity.magnitude);
    }

    void RotatePlayer(Vector3 _forwardVector)
    {
        Quaternion rotation = Quaternion.LookRotation(_forwardVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed);
    }

}

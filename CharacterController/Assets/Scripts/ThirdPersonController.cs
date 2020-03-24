using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;

    [SerializeField] float speed;

    [SerializeField] float crouchSpeed;

    [SerializeField] float runSpeed;

    [SerializeField] float airSpeed;

    [SerializeField] float jumpImpulse;

    [SerializeField] float rotationSpeed;

    [SerializeField] float timerBeforeRunning;

    public bool isInAutoPilot;

    float currentSpeed;
    float verticalInput;
    float horizontalInput;
    float timerRun = 0.0f;

    Vector3 retainVelocity;

    bool jumpInput;
    bool runInput;
    bool isRunning;
    bool crouchInput;
    bool firstTimeCrouch = true;
    bool firstTimeStandUp = true;

    Vector3 velocity;
    Vector3 direction;

    Quaternion inputRotation;
    CapsuleCollider capsuleCollider;

    enum PlayerState
    {
        running,
        standUp,
        crouch
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("missing rigidbody");
        }
        animator = GetComponent<Animator>();
        isInAutoPilot = false;
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInAutoPilot)
        {
            GetInput();

            if (IsGrounded())
            {
                if (!crouchInput) MovementOnGround();
                else Crouch();
            }
            else
                MovementInTheAir();
        }
        else
        {
            RotatePlayerWhenAuto();
        }
    }

    private void LateUpdate()
    {
        SetAnimation();
    }

    //with walk to run
    //void MovementOnGround()
    //{
    //    velocity = rb.velocity;

    //    //velocity.x = 0.0f;

    //    if (verticalInput != 0.0f || horizontalInput != 0.0f)
    //    {
    //        RotatePlayer();

    //        if (isRunning)
    //            currentSpeed = currentSpeed >= runSpeed ? runSpeed : currentSpeed + Time.deltaTime * 5.0f;
    //        else
    //            currentSpeed = speed;


    //        if (!isRunning)
    //            timerRun += Time.deltaTime;

    //        if (timerRun >= timerBeforeRunning && !isRunning)
    //        {
    //            isRunning = true;
    //            wasRunning = true;
    //        }
    //    }
    //    else
    //    {
    //        currentSpeed = currentSpeed <= 0.0f ? 0.0f : currentSpeed - Time.deltaTime * 10.0f;
    //        isRunning = false;
    //        timerRun = 0.0f;
    //    }

    //    //move character in the direction it looks
    //    velocity = transform.forward * currentSpeed;

    //    //clamp speed at it maximum value
    //    velocity = Vector3.ClampMagnitude(velocity, isRunning ? runSpeed : speed);

    //    if (jumpInput && IsGrounded())
    //    {
    //        isRunning = false;
    //        Jump();
    //    }

    //    rb.velocity = velocity;//transform.TransformDirection(velocity);
    //    //animator.SetBool("isJumping", false);
    //}

    void MovementOnGround()
    {
        if (firstTimeStandUp)
            ChangeCollider(PlayerState.standUp);
        if (verticalInput != 0.0f || horizontalInput != 0.0f)
        {
            RotatePlayer();

            if (runInput)
            {
                currentSpeed = runSpeed;
                isRunning = true;
            }
            else
            {
                currentSpeed = speed;
                isRunning = false;
            }
            //move character in the direction it looks
            velocity = direction * currentSpeed;
            //clamp speed at it maximum value
            velocity = Vector3.ClampMagnitude(velocity, currentSpeed);
        }
        else
        {
            velocity = Vector3.zero;
            isRunning = false;
        }


        rb.velocity = velocity;

        if (jumpInput)
        {
            Jump();
        }

    }

    //player can move while in the air but with less manoeuvrability
    void MovementInTheAir()
    {
        if (verticalInput != 0.0f || horizontalInput != 0.0f)
        {
            RotatePlayer();
            //move character in the direction it looks
            velocity = direction * airSpeed;
            float yVelocity = rb.velocity.y;
            velocity.y = 0.0f;
            //clamp speed at it maximum value
            velocity = Vector3.ClampMagnitude(velocity, airSpeed);
            velocity.y = yVelocity;
            //keep velocity from ground
            velocity += retainVelocity;
            rb.velocity = velocity;
        }
    }

    //rotate the character in the direction it will move
    void RotatePlayer()
    {
        //calculate next rotation
        Quaternion rotation = Quaternion.LookRotation(new Vector3(horizontalInput, 0.0f, verticalInput));
        Vector3 angle = rotation.eulerAngles;
        //add camera's rotation to the next rotation
        angle.y += Camera.main.transform.eulerAngles.y;
        rotation = Quaternion.Euler(angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        inputRotation = rotation;
        //calculate the new direction
        direction = inputRotation * Vector3.forward;
    }

    void RotatePlayerWhenAuto()
    {
        Quaternion rotation = Quaternion.LookRotation(rb.velocity);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    //add force opposite to gravity to make player jump
    void Jump()
    {
        float yVelocity = Mathf.Abs(Physics.gravity.y) / 7.0f * jumpImpulse;
        rb.AddForce(Vector3.up * yVelocity, ForceMode.VelocityChange);
        //keep current velocity to use it in the air
        retainVelocity = rb.velocity;
        animator.SetTrigger("isJumping");
    }

    void Crouch()
    {
        if (firstTimeCrouch)
            ChangeCollider(PlayerState.crouch);
        if (isRunning)
        {
            //if player is running before crouch keep its velocity and make him glide
            currentSpeed = currentSpeed <= 0.0f ? 0.0f : currentSpeed - Time.deltaTime * 3.0f;
            velocity = direction * currentSpeed;
            if (currentSpeed <= 0.0f)
                isRunning = false;
        }
        else if (verticalInput != 0.0f || horizontalInput != 0.0f)
        {
            RotatePlayer();
            //move character in the direction it looks
            velocity = direction * crouchSpeed;

            //clamp speed at it maximum value
            velocity = Vector3.ClampMagnitude(velocity, crouchSpeed);
        }
        else
        {
            velocity = Vector3.zero;
        }
        rb.velocity = velocity;
    }

    void ChangeCollider(PlayerState playerState)
    {
        Vector3 scale = transform.localScale;
        //change player scale to show it is crouch
        //change size of its collider
        if (playerState == PlayerState.crouch)
        {
            scale.y = 1.0f;
            transform.localScale = scale;
            capsuleCollider.height = 0.5f;
            firstTimeCrouch = false;
            firstTimeStandUp = true;
        }
        //same but when standUp
        else if (playerState == PlayerState.standUp)
        {
            scale.y = 2.0f;
            transform.localScale = scale;
            capsuleCollider.height = 1.0f;
            firstTimeCrouch = true;
            firstTimeStandUp = false;
        }
    }

    void GetInput()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetButtonDown("Jump");
        crouchInput = Input.GetButton("Crouch");
        runInput = Input.GetButton("Run");
    }

    void SetAnimation()
    {
        animator.SetFloat("velocity", rb.velocity.magnitude);
        animator.SetFloat("yVelocity", Mathf.Abs(rb.velocity.y));
        animator.SetBool("isRunning", isRunning);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }

    bool IsGrounded()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.05f);

        return colliders.Length >= 2;
    }
}

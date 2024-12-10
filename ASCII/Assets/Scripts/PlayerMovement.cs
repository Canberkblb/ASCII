using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private bl_Joystick joystick;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float inputThreshold = 1f;

    private CharacterController characterController;
    private Animator animator;
    private Vector3 moveDirection;
    private Vector3 velocity;
    
    public bool canMove = true;
    private bool isRunning = false;
    private bool isIdle = true;
    private bool isBoringIdle = false;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!canMove)
        {
            return;}
       
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;
        
        if (Mathf.Abs(horizontal) < inputThreshold) horizontal = 0f;
        if (Mathf.Abs(vertical) < inputThreshold) vertical = 0f;

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        UpdateAnimationState();
        
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        
        if (characterController.isGrounded)
        {
            velocity.y = 0f;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
        characterController.Move(move + velocity * Time.fixedDeltaTime);
    }

    private void UpdateAnimationState()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            isRunning = true;
            isIdle = false;
            isBoringIdle = false;
        }
        else
        {
            isRunning = false;
            isIdle = true;
        }
        
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isBoringIdle", isBoringIdle);
    }
    
    public void StopMovement()
    {
        canMove = false;
        moveDirection = Vector3.zero;
        velocity = Vector3.zero;
        characterController.Move(Vector3.zero);
        animator.SetBool("isRunning", false);
        animator.SetBool("isIdle", true);
        animator.SetBool("isBoringIdle", false);
    }
}
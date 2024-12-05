using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private bl_Joystick joystick;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float inputThreshold = 1f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector3 velocity;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;
        
        if (Mathf.Abs(horizontal) < inputThreshold) horizontal = 0f;
        if (Mathf.Abs(vertical) < inputThreshold) vertical = 0f;

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
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
}
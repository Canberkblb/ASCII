using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{
    [SerializeField] private bl_Joystick joystick;
    [SerializeField] private float moveSpeed = 1f;
    
    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        moveDirection = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
    }

    private void FixedUpdate()
    {
        if (moveDirection != Vector3.zero)
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }
}
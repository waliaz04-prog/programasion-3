using UnityEngine;

public class MovimientoPlayer : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;


    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (InputManager.Instance == null)
        {
            return;
        }

        Vector2 input = InputManager.Instance.Movement();
        Vector3 horizontalVelocity = transform.TransformDirection(new Vector3(input.x, 0f, input.y));
        horizontalVelocity *= ActualSpeed();

        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    public float ActualSpeed()
    {
        return InputManager.Instance.IsRunning() ? runSpeed : walkSpeed; 
    }
  
}

using Unity.VisualScripting;
using UnityEngine;

public class MovimientoPlayer : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;


    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.localRotation *new Vector3 (InputManager.Instance.Movement().x,0,InputManager.Instance.Movement ().y)*ActualSpeed();
    }

    public float ActualSpeed()
    {
        return InputManager.Instance.IsRunning() ? runSpeed : walkSpeed; 
    }
  
}

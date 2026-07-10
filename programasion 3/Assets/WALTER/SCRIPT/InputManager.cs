using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    PlayerControls inputs;

    private void Start()
    {
        inputs = new PlayerControls();

        inputs.Enable();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
           Destroy(this.gameObject);
        }
    }
    public Vector2 Movement()
    {
        return inputs.Default.Movement.ReadValue<Vector2>();
    }
    public Vector2 Look()
    {
        return inputs.Default.Look.ReadValue<Vector2>();
    }

    public bool IsShooting()
    {
        return inputs.Default.Shoot.IsPressed();
    }

    public bool IsRunning()
    {
        return inputs.Default.Run.IsPressed();
    }
}

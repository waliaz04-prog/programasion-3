using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerControls inputs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        inputs = new PlayerControls();
    }

    private void OnEnable()
    {
        inputs?.Enable();
    }

    private void OnDisable()
    {
        inputs?.Disable();
    }

    private void OnDestroy()
    {
        inputs?.Dispose();

        if (Instance == this)
        {
            Instance = null;
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

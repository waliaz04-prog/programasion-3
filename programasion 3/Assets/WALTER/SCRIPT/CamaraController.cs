using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float smoothness;

    [SerializeField] private float minAngleX = -80;
    [SerializeField] private float maxAngleX = 80;

    [SerializeField] private Transform player;

    private Vector2 smoothVelocity;

    private Vector2 currentVelocity;

    private void LateUpdate()
    {
        if (InputManager.Instance == null || player == null)
        {
            return;
        }

        Vector2 rawVelocity = InputManager.Instance.Look() * mouseSensitivity;
        float smoothing = smoothness <= 0f
            ? 1f
            : 1f - Mathf.Exp(-smoothness * Time.unscaledDeltaTime);
        smoothVelocity = Vector2.Lerp(smoothVelocity, rawVelocity, smoothing);

        currentVelocity += smoothVelocity;

        currentVelocity.y = Mathf.Clamp(currentVelocity.y, minAngleX, maxAngleX);

        transform.localRotation = Quaternion.AngleAxis(-currentVelocity.y, Vector3.right);
        player.localRotation = Quaternion.AngleAxis(currentVelocity.x, Vector3.up);
    }


}

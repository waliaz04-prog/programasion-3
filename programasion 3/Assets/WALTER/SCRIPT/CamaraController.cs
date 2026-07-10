using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float smoothness;

    [SerializeField] private float minAngleX = 80;
    [SerializeField] private float maxAngleX = 80;

    [SerializeField] private Transform player;

    private Vector2 smoothVelocity;

    private Vector2 currentVelocity;

    private void FixedUpdate()
    {
        Vector2 rawVelocity = Vector2.Scale(InputManager.Instance.Look(),Vector2.one* mouseSensitivity);
        smoothVelocity = Vector2.Lerp(smoothVelocity, rawVelocity,1/smoothness);

        currentVelocity += smoothVelocity;

        currentVelocity.y = Mathf.Clamp(currentVelocity.y,minAngleX,maxAngleX);

        transform.localRotation = Quaternion.AngleAxis(-currentVelocity.y, Vector3.right);
        player.transform.localRotation = Quaternion.AngleAxis(currentVelocity.x, Vector3.up);
    }


}

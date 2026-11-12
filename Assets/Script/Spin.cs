using UnityEngine;

public class Spin : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second around each axis (X, Y, Z).")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);

    [Tooltip("Rotate in local space (Self) or world space (World).")]
    [SerializeField] private Space space = Space.Self;

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, space);
    }
}
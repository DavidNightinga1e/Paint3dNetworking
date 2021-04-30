using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraRotateAround : MonoBehaviour
{
    private readonly Vector3 _pointOfInterest = new Vector3(0, 1f, 0);

    private const float MaxDistance = 160f;
    private const float MinDistance = 8f;

    private float GetSqrDistanceToPointOfInterest() => Vector3.SqrMagnitude(transform.position - _pointOfInterest);

    private void Start()
    {
        transform.position = _pointOfInterest;
        transform.rotation = Quaternion.identity;
        transform.Translate(0, 0, -5, Space.Self);
        transform.RotateAround(_pointOfInterest, transform.right, 17);
        transform.RotateAround(_pointOfInterest, Vector3.up, 120);
    }

    void Update()
    {
        // rotation
        if (Input.GetMouseButton(1))
        {
            var horizontal = Input.GetAxis("Mouse X");
            var vertical = Input.GetAxis("Mouse Y");

            transform.RotateAround(_pointOfInterest, Vector3.up, 3 * horizontal);
            transform.RotateAround(_pointOfInterest, transform.right, 3 * -vertical);
        }

        // zoom
        var zoom = Input.mouseScrollDelta.y;
        var distance = GetSqrDistanceToPointOfInterest();
        if (zoom > 0 && distance > MinDistance || zoom < 0 && distance < MaxDistance)
            transform.Translate(0, 0, zoom, Space.Self);
    }
}
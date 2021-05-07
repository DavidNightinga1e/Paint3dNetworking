using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRotateAround : MonoBehaviour
{
    private readonly Vector3 _pointOfInterest = new Vector3(0, 1f, 0);
    private const float ZoomSpeed = 50f;

    private const float MaxDistance = 160f; // sqr
    private const float MinDistance = 49f; // sqr

    private float GetSqrDistanceToPointOfInterest() => Vector3.SqrMagnitude(transform.position - _pointOfInterest);

    private void Start()
    {
        transform.position = _pointOfInterest;
        transform.rotation = Quaternion.identity;
        transform.Translate(0, 0, -Mathf.Sqrt(MinDistance), Space.Self); 
        transform.RotateAround(_pointOfInterest, transform.right, 18);
        transform.RotateAround(_pointOfInterest, Vector3.up, 135);
    }

    void Update()
    {
        if (Input.touchSupported)
        {
            if (Input.touchCount != 2) 
                return;
            
            var touchZoom = GetTouchZoom();

            if (touchZoom > 10f || touchZoom < -10f)
                Zoom(touchZoom * Time.deltaTime * 0.5f);

            ProcessMove();
        }
        else
        {
            Zoom(GetMouseZoom() * Time.deltaTime * ZoomSpeed);
            ProcessMove();
        }
    }

    private static float GetMouseZoom()
    {
        return Input.mouseScrollDelta.y;
    }

    private static float GetTouchZoom()
    {
        var tZero = Input.GetTouch(0);
        var tOne = Input.GetTouch(1);

        var tZeroPrevious = tZero.position - tZero.deltaPosition;
        var tOnePrevious = tOne.position - tOne.deltaPosition;

        var oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
        var currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

        return currentTouchDistance - oldTouchDistance;
    }

    private void ProcessMove()
    {
        if (Input.GetMouseButton(1) && !Input.GetMouseButtonDown(1))
        {
            var horizontal = Input.GetAxis("Mouse X");
            var vertical = Input.GetAxis("Mouse Y");

            transform.RotateAround(_pointOfInterest, Vector3.up, 3 * horizontal);
            transform.RotateAround(_pointOfInterest, transform.right, 3 * -vertical);
        }
    }

    private void Zoom(float zoom)
    {
        var distance = GetSqrDistanceToPointOfInterest();
        if (zoom > 0 && distance > MinDistance || zoom < 0 && distance < MaxDistance)
            transform.Translate(0, 0, zoom, Space.Self);
    }
}
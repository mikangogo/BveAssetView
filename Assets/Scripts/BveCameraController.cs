using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BveCameraController : MonoBehaviour
{
    private Camera TargetCamera { get; set; }
    private GameObject LookingObject { get; set; }
    private GameObject RotationOriginObject { get; set; }

    private Vector3 LastMousePosition { get; set; }
    private Vector3 StartMousePosition { get; set; }
    private float DefaultFov { get; set; }

    // Start is called before the first frame update
    private void Start()
    {
        TargetCamera = GetComponent<Camera>();
        LookingObject = GameObject.Find("AssetImporterRoot");
        RotationOriginObject = LookingObject.transform.Find("RotationOrigin").gameObject;
        DefaultFov = TargetCamera.fieldOfView;
    }

    // Update is called once per frame
    private void Update()
    {
        if (TargetCamera == null)
        {
            return;
        }


        var mousePositionDifferential = LastMousePosition - Input.mousePosition;
        var isNotMoved = mousePositionDifferential == Vector3.zero;


        if (Input.GetMouseButton(0) && !isNotMoved)
        {
            var velocity = mousePositionDifferential.magnitude * 0.1f;


            if (Input.GetKey(KeyCode.LeftShift))
            {
                TargetCamera.fieldOfView += mousePositionDifferential.y * 0.1f;
            }
            else
            {
                RotationOriginObject.transform.Rotate(new Vector3(-mousePositionDifferential.y, mousePositionDifferential.x, 0.0f), velocity, Space.World);

            }
        }


        if (Input.GetMouseButton(1) || Input.GetMouseButton(2) && !isNotMoved)
        {
            LookingObject.transform.localPosition += mousePositionDifferential * -0.01f;
        }


        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            

            LookingObject.transform.localPosition += LookingObject.transform.forward * scroll * 5.0f;
        }


        LastMousePosition = Input.mousePosition;
    }

    public void OnResetButtonClicked()
    {
        TargetCamera.fieldOfView = DefaultFov;
        LookingObject.transform.position = Vector3.zero;
        RotationOriginObject.transform.rotation = Quaternion.identity;
    }
}

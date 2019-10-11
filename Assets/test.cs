using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class Test : MonoBehaviour
{
    public Transform downLeftPoint;
    public Transform upRightPoint;

    private void Start()
    {
        var cameraController = GetComponent<ScrollCameraController>();

        Bounds bounds = new Bounds();
        bounds.SetMinMax(downLeftPoint.position, upRightPoint.position);
        cameraController.SetFocusObjectBounds(bounds);
    }
}
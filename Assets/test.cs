using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System.Linq;

public class Test : MonoBehaviour
{
    public Transform downLeftPoint;
    public Transform upRightPoint;

    public List<GameObject> SectorCenters;

    private void Start()
    {
        var cameraController = GetComponent<ScrollCameraController>();

        Bounds bounds = new Bounds();
        bounds.SetMinMax(downLeftPoint.position, upRightPoint.position);
        cameraController.SetFocusObjectBounds(bounds);
        List<Vector3> sectorCentersPositions = SectorCenters.Select(go => go.transform.position).ToList();
        cameraController.SetSectorCenters(sectorCentersPositions);
    }
}
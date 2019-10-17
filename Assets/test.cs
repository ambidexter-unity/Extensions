using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System.Linq;
using System;

public class test : MonoBehaviour
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

        //StartCoroutine(Delay(3f, () =>
        //{
        //    cameraController.AutoScrollTo(new Vector3(5f,5f,0f));
        //}));
    }

    IEnumerator Delay(float time, Action callback)
    {
        yield return new WaitForSecondsRealtime(time);
        callback.Invoke();
    }
}
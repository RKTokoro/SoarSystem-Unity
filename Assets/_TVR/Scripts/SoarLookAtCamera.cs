using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoarLookAtCamera : MonoBehaviour
{
    Camera targetCamera;
    private Transform _targetCameraTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        targetCamera = Camera.main;
        // カメラのTransformを取得
        _targetCameraTransform = targetCamera.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_targetCameraTransform);
    }
}

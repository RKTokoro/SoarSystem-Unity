using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoarBullet : MonoBehaviour
{
    public GameObject target;
    public float speed = 1.0f;

    private Transform _transform;
    
    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
    }
    
    
    private void FixedUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("target is null");
        }
        else
        {
            _transform.position = Vector3.MoveTowards(_transform.position, target.transform.position, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 当たったら消滅する
        Debug.Log("bullet hit");
        Destroy(this.gameObject);
    }
}
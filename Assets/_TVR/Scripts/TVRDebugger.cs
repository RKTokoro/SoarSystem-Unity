using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVRDebugger : MonoBehaviour
{
    private TVRSerialHandler _serialHandler;
    
    // Start is called before the first frame update
    void Start()
    {
        _serialHandler = FindObjectOfType<TVRSerialHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_serialHandler.message != null)
        {
            Debug.Log(_serialHandler.message);
        }
    }
}

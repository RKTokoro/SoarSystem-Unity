using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVRSystemManager : MonoBehaviour
{
    [SerializeField] private int fps = 72;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = fps;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

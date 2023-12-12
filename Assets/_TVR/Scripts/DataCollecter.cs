using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

public class DataCollecter : MonoBehaviour
{
    [SerializeField] private Vector3 acceleration = Vector3.forward;
    [SerializeField] private int recordFrames = 300;
    [SerializeField] private int bufferFrames = 5;
    [SerializeField] private int fps = 60;
    [SerializeField] private int imageWidth = 6;
    [SerializeField] private int imageHeight = 6;

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(OVRInput.RawButton.X) || Input.GetKeyDown(KeyCode.Space))
        {
            // CollectDataAsync(recordFrames, fps, acceleration);
            StartCoroutine(CollectDataCoroutine(recordFrames, bufferFrames, fps, acceleration));
            Debug.Log("Start recording.");
        }
    }
    
    // ------------------------------
    // merge imagerecoeder and automover into this class

    [SerializeField] private TVRFloorDataManager floorDataManager;
    [SerializeField] private TVRSoarBoard soarBoard;
    
    private double[,] _floorDataArray = new double[6, 6];
    private readonly string _baseSavePath = "RecordData"; 
    private string _currentSessionPath;
    private string _timestamp;

    private void StartNewSession()
    {
        _timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        string relativePath = Path.Combine(_baseSavePath, _timestamp);
        _currentSessionPath = Path.Combine(Application.dataPath, relativePath);

        if (!Directory.Exists(_currentSessionPath))
        {
            Directory.CreateDirectory(_currentSessionPath);
        }
        
        Debug.Log("Created new session directory at " + _currentSessionPath);
    }
    
    void UpdateArray()
    {
        if (floorDataManager.FloorData != null)
        {
            _floorDataArray = floorDataManager.FloorData;
            // Debug.Log("array updated.");
        }
        else
        {
            // Debug.LogWarning("Floor data is null.");
        }
    }

    void SaveArrayAsImage(int frameNumber)
    {
        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Point;
        
        for(int i = 0; i < imageWidth; i++)
        {
            for(int j = 0; j < imageHeight; j++)
            {
                float value = (float)_floorDataArray[i, j];
                Color color = new Color(value, value, value);
                texture.SetPixel(i, j, color);
            }
        }
        
        texture.Apply();
        
        byte[] bytes = texture.EncodeToPNG();
        
        string filepath = Path.Combine(_currentSessionPath, _timestamp + "_frame_" + frameNumber.ToString("D3") + ".png");
        File.WriteAllBytes(filepath, bytes);
        
        Destroy(texture);
    }
    
    private async Task CollectDataAsync(int recordFrameCount, int fps, Vector3 a)
    {
        StartNewSession();
        int frameCount = 0;
        
        while (frameCount < recordFrameCount)
        {
            // update floor data
            UpdateArray();
            
            // update soar board acceleration
            soarBoard.a = a;
            
            // save image
            SaveArrayAsImage(frameCount);
            Debug.Log("Saved image at " + frameCount.ToString("D3") + " frame.");
            
            // increment frame count
            frameCount++;
            await Task.Delay(1000 / fps);
        }
        
        soarBoard.a = Vector3.zero;
    }

    private IEnumerator CollectDataCoroutine(int recordFrameCount, int bufferFrameCount, int fps, Vector3 a)
    {
        StartNewSession();
        int frameCount = 0;
        while (frameCount < recordFrameCount)
        {
            soarBoard.a = a;
            if (frameCount >= bufferFrameCount)
            {
                UpdateArray();
                SaveArrayAsImage(frameCount);
                Debug.Log("Saved image at " + frameCount.ToString("D3") + " frame.");
            }
            frameCount++;
            yield return new WaitForSeconds(1.0f / fps);
        }
        soarBoard.a = Vector3.zero;
    }
}

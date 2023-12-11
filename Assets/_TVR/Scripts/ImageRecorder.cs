using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

public class ImageRecorder : MonoBehaviour
{
    private TVRFloorDataManager _floorDataManager;
    
    private double[,] array = new double[6, 6];
    private string baseSavePath = "Assets/RecordData"; 
    private string currentSessionPath;
    private int frameCount = 0;
    private bool startRecording = false;
    private float elapsedTime = 0f;
    [SerializeField] private float recordingTime = 5f;

    private void Start()
    {
        Application.targetFrameRate = 72;
        _floorDataManager = FindObjectOfType<TVRFloorDataManager>();
    }

    void Update()
    {
        /*
        if (startRecording)
        {
            // 配列を更新
            UpdateArray();

            // 配列を画像に変換して保存
            SaveArrayAsImage(frameCount);

            // フレームカウンタをインクリメント
            frameCount++;

            // 360枚の画像が保存されたら停止
            if (frameCount >= 360)
            {
                startRecording = false;
                Debug.Log("Recording stopped.");
            }
        }
        */
    }

    void StartNewSession()
    {
        startRecording = true;
        elapsedTime = 0f;
        frameCount = 0;

        // セッションのタイムスタンプに基づいてディレクトリ名を作成
        string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        string relativePath = Path.Combine("RecordData", timestamp);
        currentSessionPath = Path.Combine(Application.dataPath, relativePath);

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(currentSessionPath))
        {
            Directory.CreateDirectory(currentSessionPath);
        }

        Debug.Log("Created new session directory at " + currentSessionPath);
    }

    void UpdateArray()
    {
        if (_floorDataManager.FloorData != null)
        {
            array = _floorDataManager.FloorData;
            Debug.Log("array updated.");
        }
        else
        {
            Debug.LogWarning("Floor data is null.");
        }
    }

    void SaveArrayAsImage(int frameNumber)
    {
        int width = 6;
        int height = 6;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Point;
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float value = (float)array[i, j];
                Color color = new Color(value, value, value);
                texture.SetPixel(i, j, color);
            }
        }

        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();

        string filePath = Path.Combine(currentSessionPath, "frame_" + frameNumber.ToString("D3") + ".png");
        File.WriteAllBytes(filePath, bytes);
        // Debug.Log("image saved.");

        Destroy(texture);
    }

    async Task SaveArrayAsImageAsync()
    {
        int width = 6;
        int height = 6;
        
        while (frameCount < 360) 
        {
            // arrayを更新
            UpdateArray();
            
            SaveArrayAsImage(frameCount);
            
            // フレームカウンタをインクリメント
            frameCount++;
            
            // 1/60 秒待機
            await Task.Delay(TimeSpan.FromSeconds(1.0 / 60));
        }
        
        // whileを抜けたら終了
        Debug.Log("Recording stopped.");
    }
    
    public void StartRecording()
    {
        StartNewSession();
        Debug.Log("Recording started.");
        SaveArrayAsImageAsync();
    }
}
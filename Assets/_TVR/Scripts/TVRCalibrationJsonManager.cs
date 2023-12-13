using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Serialization;

[System.Serializable]
public class SerializableFloorData
{
    public double[] p;

    public SerializableFloorData(double[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        p = new double[rows * cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                p[i * cols + j] = array[i, j];
            }
        }
    }
}

[System.Serializable]
public class SerializableCalibrationData
{
    public SerializableFloorData baseLine;
    public SerializableFloorData max;
    public SerializableFloorData min;

    public SerializableCalibrationData(CalibrationData calibrationData)
    {
        baseLine = new SerializableFloorData(calibrationData.baseLine.p);
        max = new SerializableFloorData(calibrationData.max.p);
        min = new SerializableFloorData(calibrationData.min.p);
    }
}

public class TVRCalibrationJsonManager : MonoBehaviour
{
    [SerializeField] private TVRFloorDataManager floorDataManager;
    
    // Start is called before the first frame update
    void Start()
    {
        if(floorDataManager == null)
        {
            floorDataManager = FindObjectOfType<TVRFloorDataManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            SaveCalibrationDataToJson();
        }
    }
    
    private void SaveCalibrationDataToJson()
    {
        // CalibrationDataをSerializableCalibrationDataに変換
        SerializableCalibrationData serializableData = new SerializableCalibrationData(floorDataManager.calibrationData);
        
        // JSONデータを生成
        // string jsonData = JsonUtility.ToJson(container, true);
        string jsonData = JsonUtility.ToJson(serializableData, true);

        // 現在の時刻を取得し、ファイル名を生成
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = "calibrationData_" + timestamp + ".json";
        string filePath = Path.Combine(Application.dataPath, "calibrationData", fileName);

        // ファイルに書き込み
        File.WriteAllText(filePath, jsonData);
        Debug.Log("Calibration data saved to: " + filePath);
    }
}

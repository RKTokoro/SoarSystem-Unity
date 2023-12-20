using System;
using UnityEngine;
using System.IO;

public class SoarJsonManager : MonoBehaviour
{
    [SerializeField] private SoarFloorDataManager floorDataManager;
    
    public TextAsset jsonFile;
    private CalibrationData _calibrationData;
    
    // Start is called before the first frame update
    void Start()
    {
        if(floorDataManager == null)
        {
            floorDataManager = FindFirstObjectByType<SoarFloorDataManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            SaveCalibrationDataToJson();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadCalibrationDataFromJson();
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
    
    private void LoadCalibrationDataFromJson()
    {
        if (jsonFile != null)
        {
            // JSONファイルの内容を文字列として取得
            string jsonData = jsonFile.text;

            // 文字列をSerializableCalibrationDataにデシリアライズ
            SerializableCalibrationData loadedData = JsonUtility.FromJson<SerializableCalibrationData>(jsonData);

            // SerializableCalibrationDataをCalibrationDataに変換
            _calibrationData = ConvertToCalibrationData(loadedData);

            // CalibrationDataをTVRFloorDataManagerにセット
            floorDataManager.calibrationData = _calibrationData;
        }
    }
    
    private CalibrationData ConvertToCalibrationData(SerializableCalibrationData serializableData)
    {
        CalibrationData newCalibrationData = new CalibrationData();

        // baseLineの変換
        newCalibrationData.baseLine.p = ConvertTo2DArray(serializableData.baseLine.p, 6, 6);

        // maxの変換
        newCalibrationData.max.p = ConvertTo2DArray(serializableData.max.p, 6, 6);

        // minの変換
        newCalibrationData.min.p = ConvertTo2DArray(serializableData.min.p, 6, 6);

        return newCalibrationData;
    }

    private double[,] ConvertTo2DArray(double[] array, int rows, int cols)
    {
        double[,] twoDArray = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                twoDArray[i, j] = array[i * cols + j];
            }
        }

        return twoDArray;
    }
}

[Serializable]
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

[Serializable]
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class TVRDebugger : MonoBehaviour
{
    public TVRSerialHandler serialHandler;
    public TVRFloorDataManager floorDataManager;
    public TextMeshProUGUI floorDataText;
    
    // Start is called before the first frame update
    void Start()
    {
        serialHandler = FindObjectOfType<TVRSerialHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        DisplayData();
    }
    
    void DisplayData()
    {
        floorDataText.text = "Lorem ipsum dolor sit amet,";
        if (floorDataManager != null && floorDataText != null)
        {
            string dataString = "FloorData:\n" + MatrixToString(floorDataManager.FloorData) +
                                "\nFloorDataRaw:\n" + MatrixToString(floorDataManager._floorDataRaw) +
                                "\nCalibrationData:\n" + MatrixToString(floorDataManager._calibrationData[0]);

            floorDataText.text = dataString;
        }
    }

    string MatrixToString(double[,] matrix)
    {
        string result = "";
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                result += matrix[i, j].ToString("F2") + " ";
            }
            result += "\n";
        }
        return result;
    }
}

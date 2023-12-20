using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;

public class TVRDebugger : MonoBehaviour
{
    public SoarFloorDataManager floorDataManager;
    public MLImageGenerator mlImageGenerator;
    public TextMeshProUGUI floorDataText;
    public TextMeshProUGUI trackingDataText;
    public RawImage floorImageRawImage;
    
    [SerializeField] private GameObject centerEyeAnchor, leftHandAnchor, rightHandAnchor;
    private Transform _headTransform, _leftHandTransform, _rightHandTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        if (floorDataManager == null)
        {
            floorDataManager = FindFirstObjectByType<SoarFloorDataManager>();
        }
        
        _headTransform = centerEyeAnchor.GetComponent<Transform>();
        _leftHandTransform = leftHandAnchor.GetComponent<Transform>();
        _rightHandTransform = rightHandAnchor.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        DisplayData();
        DisplayImage();
    }
    
    void DisplayData()
    {
        floorDataText.text = "Lorem ipsum dolor sit amet,";
        if (floorDataManager != null && floorDataText != null)
        {
            string dataString = "FloorData:\n" + MatrixToString(floorDataManager.floorData.p) +
                                "\nFloorDataRaw:\n" + MatrixToString(floorDataManager.floorDataRaw.p) +
                                "\nCalibrationDataMin:\n" + MatrixToString(floorDataManager.calibrationData.min.p) +
                                "\nCalibrationDataMax:\n" + MatrixToString(floorDataManager.calibrationData.max.p);

            floorDataText.text = dataString;
        }
        
        trackingDataText.text = "Lorem ipsum dolor sit amet,";
        if (trackingDataText != null)
        {
            string dataString = "Head:\n" + _headTransform.localPosition.ToString("F2") + _headTransform.localRotation.eulerAngles.ToString("F2") +
                                "\nLeftHand:\n" + _leftHandTransform.localPosition.ToString("F2") + _leftHandTransform.localRotation.eulerAngles.ToString("F2") +
                                "\nRightHand:\n" + _rightHandTransform.localPosition.ToString("F2") + _rightHandTransform.localRotation.eulerAngles.ToString("F2");

            trackingDataText.text = dataString;
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

    void DisplayImage()
    {
        // floorImageRawImage.texture = floorDataManager.floorImageTexture;
        floorImageRawImage.texture = mlImageGenerator.inputTexture;
    }
}

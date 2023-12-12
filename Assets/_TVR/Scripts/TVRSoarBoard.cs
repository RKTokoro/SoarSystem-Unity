using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TVRSoarBoard : MonoBehaviour
{
    public bool isSoaring = false;
    public bool isAscending = false;
    public bool isDescending = false;
    public bool isBrakingPosition = false;
    public bool isBrakingRotation = false;
    public bool invertVerticalMovement = false;
    public bool isAutonomous = false;
    
    private Transform _transform;

    private GameObject _head;
    private Transform _headTransform;
    public float headHeightDefault = 1.0f;
    public float ascendThreshold = 0.1f;
    public float descendThreshold = 0.3f;
    
    private GameObject[,] _modules = new GameObject[2,2];
    [SerializeField] private GameObject moduleFL;
    [SerializeField] private GameObject moduleFR;
    [SerializeField] private GameObject moduleBL;
    [SerializeField] private GameObject moduleBR;
    private Renderer[,] _moduleRenderers = new Renderer[2,2];
    
    private static TVRFloorDataManager _floorDataManager;
    
    private double[,] _pressuresMean = new double[2, 2];
    public double[,] _pressuresNormalized = new double[2, 2];
    private double[,] _pressuresMeanMax = new double[2, 2];
    private double[,] _pressuresMeanMin = new double[2, 2];
    
    private static bool isCalibrationSequence = false;
    
    public Vector3 forceLF, forceRF, forceLB, forceRB;
    
    // variables for position
    public float m = 1.0f;  // mass
    public Vector3 v;  // velocity
    public Vector3 a;  // acceleration
    public float ka = 1.0f;
    public float av = 1.0f;  // vertical acceleration
    
    // variables for rotation
    // soar board only rotates around y axis
    [SerializeField] private float _moduleRadius = 0.5f;
    public float momentOfInertia = 1.0f;  // moment of inertia
    public float w;  // angular velocity
    public float b; // angular acceleration
    public float kb = 1.0f;
    
    public float brakePositionForce = 1.0f;
    public float brakeRotationForce = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
        
        _floorDataManager = FindObjectOfType<TVRFloorDataManager>();
        
        _head = GameObject.Find("CenterEyeAnchor");
        _headTransform = _head.GetComponent<Transform>();
        
        _modules[0,0] = moduleFL;
        _modules[0,1] = moduleFR;
        _modules[1,0] = moduleBL;
        _modules[1,1] = moduleBR;
        
        _moduleRenderers[0,0] = moduleFL.GetComponent<Renderer>();
        _moduleRenderers[0,1] = moduleFR.GetComponent<Renderer>();
        _moduleRenderers[1,0] = moduleBL.GetComponent<Renderer>();
        _moduleRenderers[1,1] = moduleBR.GetComponent<Renderer>();
    }
    
    // Update is called once per frame
    void Update()
    {
        UpdateMeanPressures();
        UpdateModuleColor();
        DetectHeadMovement();
        
        isBrakingPosition = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
        isBrakingRotation = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        
        if (isSoaring || isAutonomous)
        {
            Soar();
        }

        if (isCalibrationSequence)
        {
            Calibrate();
        }
        
        if(Input.GetKeyDown(KeyCode.Space)  || OVRInput.GetDown(OVRInput.RawButton.A))
        {
            isSoaring = !isSoaring;
            Reset();
        }
        
        if(Input.GetKeyDown(KeyCode.V))
        {
            isCalibrationSequence = true;
            Debug.Log("SoarBoard Calibration sequence started.");
        }
        
        if(OVRInput.GetDown(OVRInput.RawButton.B))
        {
            headHeightDefault = _headTransform.localPosition.y;
            Debug.Log("Head height reset.");
        }
    }
    
    private void UpdateMeanPressures()
    {
        // モジュールごとのセンサーの数
        int sensorsPerModule = 3;

        // 左前
        _pressuresMean[0, 0] = 
            CalculateAveragePressure(
                _floorDataManager.FloorData,
                0, 0, sensorsPerModule, sensorsPerModule);
        // 右前
        _pressuresMean[0, 1] = 
            CalculateAveragePressure(
                _floorDataManager.FloorData, 
                0, sensorsPerModule, sensorsPerModule, TVRFloorDataManager.Columns);
        // 左後
        _pressuresMean[1, 0] = 
            CalculateAveragePressure(_floorDataManager.FloorData, 
                sensorsPerModule, 0, TVRFloorDataManager.Rows, sensorsPerModule);
        // 右後
        _pressuresMean[1, 1] = 
            CalculateAveragePressure(_floorDataManager.FloorData, 
                sensorsPerModule, sensorsPerModule, TVRFloorDataManager.Rows, TVRFloorDataManager.Columns);
    }

    private static double CalculateAveragePressure(double[,] floorData, int startRow, int startColumn, int endRow, int endColumn)
    {
        double total = 0;
        int count = 0;

        for (int i = startRow; i < endRow; i++)
        {
            for (int j = startColumn; j < endColumn; j++)
            {
                total += floorData[i, j];
                count++;
            }
        }

        return total / count;
    }
    
    private void UpdateModuleColor()
    {
        for (int i = 0; i < _modules.GetLength(0); i++)
        {
            for (int j = 0; j < _modules.GetLength(1); j++)
            {
                _moduleRenderers[i, j].material.color = Color.Lerp(Color.white, Color.red, (float)_pressuresMean[i, j]);
            }
        }
    }

    private void Reset()
    {
        _transform.position = Vector3.zero;
        _transform.rotation = Quaternion.identity;
        
        a = Vector3.zero;
        v = Vector3.zero;

        b = 0.0f;
        w = 0.0f;
    }

    private void Soar()
    {
        _pressuresNormalized = Normalize(_pressuresMean, _pressuresMeanMin, _pressuresMeanMax);
        
        if (!isAutonomous)
        {
            CalcForces(_pressuresNormalized);
            a = CalcAcceleration(v);
            b = CalcAngularAcceleration(w);
        }
        
        v = CalcVelocity(v, a);
        w = CalcAngularVelocity(w, b);
        
        Move();
        Rotate();
    }

    private void CalcForces(double[,] floorData)
    {
        Quaternion rot = _transform.rotation;
        
        forceLF = rot * new Vector3(
            -(float)floorData[0, 0],
            0.0f,
            (float)floorData[0, 0]
        );
        
        forceRF = rot * new Vector3(
            (float)floorData[0, 1],
            0.0f,
            (float)floorData[0, 1]
        );
        
        forceLB = rot * new Vector3(
            -(float)floorData[1, 0],
            0.0f,
            -(float)floorData[1, 0]
        );
        
        forceRB = rot * new Vector3(
            (float)floorData[1, 1],
            0.0f,
            -(float)floorData[1, 1]
        );
    }
    
    private Vector3 CalcAcceleration(Vector3 v)
    {
        Vector3 acc = Vector3.zero;
        
        Vector3 totalForce = forceLF + forceRF + forceLB + forceRB;
        acc.x = totalForce.x / m - ka * v.x;
        acc.z = totalForce.z / m - ka * v.z;
        
        // vertical movement
        if (!isAutonomous)
        {
            if (isAscending)
            {
                acc.y = av;
            }
            else if (isDescending)
            {
                acc.y = -av;
            }
            else
            {
                acc.y = 0.0f;
            }
        }

        acc.y -= ka * v.y;
        
        if (isBrakingPosition)
        {
            acc -= brakePositionForce * v;
        }
        
        return acc;
    }

    private Vector3 CalcVelocity(Vector3 pv, Vector3 a)
    {
        Vector3 v = pv + a * Time.deltaTime;
        return v;
    }
    
    private float CalcAngularAcceleration(float w)
    {
        float acc = 0.0f;

        acc = ((2.0f * forceLF.magnitude)
               - (2.0f * forceRF.magnitude)
               - (2.0f * forceLB.magnitude) 
               + (2.0f * forceRB.magnitude)) / (4 * momentOfInertia) - kb * w;
        
        if (isBrakingRotation)
        {
            acc -= brakeRotationForce * w;
        }
        
        return acc;
    }
    
    private float CalcAngularVelocity(float pw, float a)
    {
        float w = pw + a * Time.deltaTime;
        return w;
    }

    private void Move()
    {
        _transform.position += v * Time.deltaTime;
    }
    
    private void Rotate()
    {
        _transform.Rotate(0, w * Mathf.Rad2Deg * Time.deltaTime, 0);
    }

    private void Calibrate()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                double currentValue = _pressuresMean[i, j];

                // 最小値を更新
                if (currentValue < _pressuresMeanMin[i, j] || _pressuresMeanMin[i, j] == 0)
                {
                    _pressuresMeanMin[i, j] = currentValue;
                }

                // 最大値を更新
                if (currentValue > _pressuresMeanMax[i, j])
                {
                    _pressuresMeanMax[i, j] = currentValue;
                }
            }
        }
        
        if(Input.GetKeyDown(KeyCode.S))
        {
            isCalibrationSequence = false;
            Debug.Log("SoarBoard Calibration sequence ended.");
        }
    }

    private double[,] Normalize(double[,] data, double[,] minData, double[,] maxData)
    {
        int rows = data.GetLength(0);
        int columns = data.GetLength(1);
        double[,] normalizedData = new double[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                double min = minData[i, j]; // 最小値
                double max = maxData[i, j]; // 最大値

                // 除算の分母が0にならないようにチェック
                if (max - min != 0)
                {
                    normalizedData[i, j] = (data[i, j] - min) / (max - min);
                }
                else
                {
                    // 最大値とベースラインが同じ場合、値を0または1に設定
                    normalizedData[i, j] = (data[i, j] == max) ? 1 : 0;
                }
            }
        }

        return normalizedData;
    }

    private void DetectHeadMovement()
    {
        if (!invertVerticalMovement)
        {
            // if head is moving up, ascend
            if(_headTransform.localPosition.y > headHeightDefault + ascendThreshold)
            {
                isDescending = false;
                isAscending = true;
            }
            // if head is moving down, descend
            else if(_headTransform.localPosition.y < headHeightDefault - descendThreshold)
            {
                isAscending = false;
                isDescending = true;
            }
            // if head is default position, stop ascending and descending
            else
            {
                isAscending = false;
                isDescending = false;
            }
        }
        else
        {
            // if head is moving up, descend
            if(_headTransform.localPosition.y > headHeightDefault + ascendThreshold)
            {
                isAscending = false;
                isDescending = true;
            }
            // if head is moving down, ascend
            else if(_headTransform.localPosition.y < headHeightDefault - descendThreshold)
            {
                isDescending = false;
                isAscending = true;
            }
            // if head is default position, stop ascending and descending
            else
            {
                isAscending = false;
                isDescending = false;
            }
        }
    }
}

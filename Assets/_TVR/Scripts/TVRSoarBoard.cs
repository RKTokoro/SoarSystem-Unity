using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TVRSoarBoard : MonoBehaviour
{
    public bool isSoaring = false;
    
    private Transform _transform;
    
    private GameObject[,] _modules = new GameObject[2,2];
    [SerializeField] private GameObject moduleFL;
    [SerializeField] private GameObject moduleFR;
    [SerializeField] private GameObject moduleBL;
    [SerializeField] private GameObject moduleBR;
    private Renderer[,] _moduleRenderers = new Renderer[2,2];
    
    private static TVRFloorDataManager _floorDataManager;
    
    private static double[,] _meanPressures = new double[2, 2];
    
    public float m = 1.0f;  // mass
    public Vector3 v;  // velocity
    public Vector3 a;  // acceleration
    public float ka = 1.0f;
    
    // variables for rotation
    // soar board only rotates around y axis
    [SerializeField] private float _moduleRadius = 0.5f;
    public float momentOfInertia = 1.0f;  // moment of inertia
    public float w;  // angular velocity
    public float b; // angular acceleration
    public float kb = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
        
        _floorDataManager = FindObjectOfType<TVRFloorDataManager>();
        
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
        
        if (isSoaring)
        {
            Soar();
        }
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            isSoaring = !isSoaring;
            Reset();
        }
    }
    
    private static void UpdateMeanPressures()
    {
        // モジュールごとのセンサーの数
        int sensorsPerModule = 3;

        // 左前
        _meanPressures[0, 0] = 
            CalculateAveragePressure(
                _floorDataManager.FloorData,
                0, 0, sensorsPerModule, sensorsPerModule);
        // 右前
        _meanPressures[0, 1] = 
            CalculateAveragePressure(
                _floorDataManager.FloorData, 
                0, sensorsPerModule, sensorsPerModule, TVRFloorDataManager.Columns);
        // 左後
        _meanPressures[1, 0] = 
            CalculateAveragePressure(_floorDataManager.FloorData, 
                sensorsPerModule, 0, TVRFloorDataManager.Rows, sensorsPerModule);
        // 右後
        _meanPressures[1, 1] = 
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
                _moduleRenderers[i, j].material.color = Color.Lerp(Color.white, Color.red, (float)_meanPressures[i, j]);
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
        a = CalcAcceleration(v);
        v = CalcVelocity(v, a);

        b = CalcAngularAcceleration(w);
        w = CalcAngularVelocity(w, b);
        
        Move();
        Rotate();
    }
    
    private Vector3 CalcAcceleration(Vector3 v)
    {
        Vector3 acc = Vector3.zero;
        
        acc.x = 
            ((float)(-_meanPressures[0, 0] - _meanPressures[1, 0] + _meanPressures[0, 1] + _meanPressures[1, 1]) / m) 
            - ka * v.x;
        acc.y = 0.0f;  // write it later
        acc.z = 
            ((float)(_meanPressures[0, 0] - _meanPressures[1, 0] + _meanPressures[0, 1] - _meanPressures[1, 1]) / m) 
            - ka * v.z;
        
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

        acc = (((2.0f * (float)_meanPressures[0, 0]) 
               - (2.0f * (float)_meanPressures[0, 1]) 
               - (2.0f * (float)_meanPressures[1, 0]) 
               + (2.0f * (float)_meanPressures[1, 1])) 
              / (4 * momentOfInertia)) 
              - kb * w;
        
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
        _transform.Rotate(0, w * Time.deltaTime, 0);
    }
}

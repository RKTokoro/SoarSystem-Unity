using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVRSoarBoard : MonoBehaviour
{
    private GameObject[,] _modules = new GameObject[2,2];
    [SerializeField] private GameObject moduleFL;
    [SerializeField] private GameObject moduleFR;
    [SerializeField] private GameObject moduleBL;
    [SerializeField] private GameObject moduleBR;
    private Renderer[,] _moduleRenderers = new Renderer[2,2];
    
    private static TVRFloorDataManager _floorDataManager;
    
    private static double[,] _meanPressures = new double[2, 2];
    
    // Start is called before the first frame update
    void Start()
    {
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
}

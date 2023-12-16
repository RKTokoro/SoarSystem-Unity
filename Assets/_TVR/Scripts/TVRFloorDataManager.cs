using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.IO;

public class FloorData
{
    public double[,] p = new double[6,6];
}

public class CalibrationData
{
    public FloorData baseLine = new FloorData();
    public FloorData max = new FloorData();
    public FloorData min = new FloorData();
}

public class TVRFloorDataManager : MonoBehaviour
{
    // singleton
    public static TVRFloorDataManager floorDataManager;
    private void Awake()
    {
        if (floorDataManager == null)
        {
            
            floorDataManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /* ----------------------------------*/
    
    [SerializeField] private TVRParser parser;
    
    public static readonly int Rows = 6;
    public static readonly int Columns = 6;

    [HideInInspector] public FloorData floorData = new FloorData();
    public FloorData floorDataRaw = new FloorData();
    public CalibrationData calibrationData = new CalibrationData();
    // calibrationData[行, 列, {ベースライン値, 最小値, 最大値}]

    public Texture2D floorImageTexture;
    
    private static bool isCalibrationSequence = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (parser == null)
        {
            parser = FindObjectOfType<TVRParser>();
        }
        
        InitializeCalibrationData();
    }
    
    private void InitializeCalibrationData()
    {
        // initialize floor data
        floorData.p = new double[Rows, Columns];
        
        // initialize calibration data
        calibrationData.baseLine = new FloorData();
        calibrationData.max = new FloorData();
        calibrationData.min = new FloorData();
        
        calibrationData.baseLine.p = new double[Rows, Columns];
        calibrationData.max.p = new double[Rows, Columns];
        calibrationData.min.p = new double[Rows, Columns];
        
        // ベースライン値と最大値を負の無限大で初期化
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                calibrationData.max.p[row, col] = double.NegativeInfinity;
            }
        }

        // 最小値を正の無限大で初期化
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                calibrationData.min.p[row, col] = double.PositiveInfinity;
            }
        }

        floorImageTexture = new Texture2D(Rows, Columns, TextureFormat.R16, false);
        floorImageTexture.filterMode = FilterMode.Point;
    }

    // Update is called once per frame
    void Update()
    {
        // update floor data
        floorDataRaw = parser.floorData;
        floorData.p = SortFloorData(floorDataRaw.p);
        
        if(Input.GetKeyDown(KeyCode.C))
        {
            isCalibrationSequence = true;
            Debug.Log("Calibration sequence started.");
        }
        
        // callibration
        if (isCalibrationSequence)
        {
            Calibration();
            Debug.Log("Calibrating...");
        }
        
        // normalize
        floorData = NormalizeFloorData(floorData, calibrationData);
        
        // ignore dead cells
        IgnoreDeadCells();
        
        // update floor image texture
        UpdateFloorImageTexture();
    }
    
    private static double[,] SortFloorData(double[,] floorData)
    {
        double[,] sortedFloorData = new double[Rows, Columns];
        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                sortedFloorData[i, j] = floorData[SortMatrix[i, j, 0], SortMatrix[i, j, 1]];
            }
        }
        
        return sortedFloorData;
    }
    
    private static readonly int[,,] SortMatrix = new int[6,6,2]
    {
        {
            {5, 5}, {5, 4}, {5, 1}, {5, 0}, {4, 3}, {4, 2}
        },
        {
            {5, 3}, {5, 2}, {4, 5}, {4, 4}, {4, 1}, {4, 0}
        }, 
        {
            {3, 5}, {3, 4}, {3, 1}, {3, 0}, {2, 3}, {2, 2}
        },
        {
            {3, 3}, {3, 2}, {2, 5}, {2, 4}, {2, 1}, {2, 0}
        },
        {
            {1, 5}, {1, 4}, {1, 1}, {1, 0}, {0, 3}, {0, 2}
        },
        {
            {1, 3}, {1, 2}, {0, 5}, {0, 4}, {0, 1}, {0, 0}
        }
    };
    
    private FloorData NormalizeFloorData(FloorData floorData, CalibrationData calibrationData)
    {
        int rows = floorData.p.GetLength(0);
        int columns = floorData.p.GetLength(1);
        FloorData normalizedData = new FloorData();
        normalizedData.p = new double[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                double min = calibrationData.min.p[i, j]; // 最小値
                double max = calibrationData.max.p[i, j]; // 最大値
                
                // 除算の分母が0にならないようにチェック
                if (max - min != 0)
                {
                    normalizedData.p[i, j] = (floorData.p[i, j] - min) / (max - min);
                }
            }
        }

        return normalizedData;
    }
    
    private void Calibration()
    {
        UpdateCalibrationData();
        
        // n for neutral
        if(Input.GetKeyDown(KeyCode.N))
        {
            GetNeutralFloorData();
        }
        
        // s for save
        if(Input.GetKeyDown(KeyCode.S))
        {
            isCalibrationSequence = false;
            Debug.Log("Calibration sequence ended.");
        }
    }
    
    private void GetNeutralFloorData()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                calibrationData.baseLine.p[i, j] = floorData.p[i, j]; // ベースライン値を設定
            }
        }
    }
    
    private void UpdateCalibrationData()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                double currentValue = floorData.p[i, j];

                // 最小値を更新
                if (currentValue < calibrationData.min.p[i, j])
                {
                    calibrationData.min.p[i, j] = currentValue;
                }

                // 最大値を更新
                if (currentValue > calibrationData.max.p[i, j])
                {
                    calibrationData.max.p[i, j] = currentValue;
                }
            }
        }
    }

    private int[][] _deadCellList = new int[][]
    {
        new int[] {0, 3},
        new int[] {2, 1},
        new int[] {5, 2}
    };
    
    private void IgnoreDeadCells()
    {
        for(int i = 0; i < _deadCellList.Length; i++)
        {
            floorData.p[_deadCellList[i][0], _deadCellList[i][1]] = 0;
        }
    }
    
    private void UpdateFloorImageTexture()
    {
        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                floorImageTexture.SetPixel(i, j, new Color((float)floorData.p[i, j], (float)floorData.p[i, j], (float)floorData.p[i, j]));
            }
        }
        
        floorImageTexture.Apply();
    }
}

using UnityEngine;
using UnityEngine.Serialization;

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
    
    private TVRParser _parser;
    
    private static readonly int Rows = 6;
    private static readonly int Columns = 6;

    [HideInInspector] public double[,] FloorData { get; set; } = new double[Rows, Columns];
    private static double[,] _floorDataRaw = new double[Rows, Columns];
    private static double[,,] _calibrationData = new double[Rows, Columns, 3];
    // calibrationData[行, 列, {ベースライン値, 最小値, 最大値}]
    
    private static bool isCalibrationSequence = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _parser = FindObjectOfType<TVRParser>();
    }

    // Update is called once per frame
    void Update()
    {
        // update floor data
        _floorDataRaw = _parser.floorData;
        FloorData = SortFloorData(_floorDataRaw);
        
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
        FloorData = NormalizeFloorData(FloorData, _calibrationData);
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
    
    private double[,] NormalizeFloorData(double[,] floorData, double[,,] calibrationData)
    {
        int rows = floorData.GetLength(0);
        int columns = floorData.GetLength(1);
        double[,] normalizedData = new double[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                double baseline = calibrationData[i, j, 0];
                double max = calibrationData[i, j, 2];

                // 除算の分母が0にならないようにチェック
                if (max - baseline != 0)
                {
                    normalizedData[i, j] = (floorData[i, j] - baseline) / (max - baseline);
                }
                else
                {
                    // 最大値とベースラインが同じ場合、値を0または1に設定
                    normalizedData[i, j] = (floorData[i, j] == max) ? 1 : 0;
                }
            }
        }

        return normalizedData;
    }

    
    private static void GetNeutralFloorData()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                _calibrationData[i, j, 0] = _floorDataRaw[i, j]; // ベースライン値を設定
            }
        }
    }
    
    private double[,] SubtractMatrices(double[,] matrixA)
    {
        double[,] result = new double[Rows, Columns];

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result[i, j] = matrixA[i, j] - _calibrationData[i, j, 0]; // ベースライン値を使用
            }
        }

        return result;
    }

    private static void Calibration()
    {
        // n for neutral
        if(Input.GetKeyDown(KeyCode.N))
        {
            GetNeutralFloorData();
        }
        
        UpdateCalibrationData();
        
        // s for save
        if(Input.GetKeyDown(KeyCode.S))
        {
            isCalibrationSequence = false;
            Debug.Log("Calibration sequence ended.");
        }
    }
    
    private static void UpdateCalibrationData()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                double currentValue = _floorDataRaw[i, j];

                // 最小値を更新
                if (currentValue < _calibrationData[i, j, 1] || _calibrationData[i, j, 1] == 0)
                {
                    _calibrationData[i, j, 1] = currentValue;
                }

                // 最大値を更新
                if (currentValue > _calibrationData[i, j, 2])
                {
                    _calibrationData[i, j, 2] = currentValue;
                }
            }
        }
    }
}

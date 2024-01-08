using UnityEngine;

public class SoarFloorDataManager : MonoBehaviour
{
    [SerializeField] private SoarParser parser;
    
    public static readonly int Rows = 6;
    public static readonly int Columns = 6;
    
    public FloorData floorData;
    [HideInInspector] public Texture2D floorImageTexture;
    
    public FloorData floorDataRaw;
    public CalibrationData calibrationData;
    
    private static bool _isCalibrationSequence;
    
    // Start is called before the first frame update
    void Start()
    {
        if (parser == null)
        {
            parser = FindFirstObjectByType<SoarParser>();
        }

        // initialize floor data
        floorData = new FloorData();
        floorData.p = new double[Rows, Columns];
        floorDataRaw = new FloorData();
        floorDataRaw.p = new double[Rows, Columns];
        
        InitializeCalibrationData();
    }
    
    private void InitializeCalibrationData()
    { 
        // initialize calibration data
        calibrationData = new CalibrationData();
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
            _isCalibrationSequence = true;
            Debug.Log("Calibration sequence started.");
        }
        
        // callibration
        if (_isCalibrationSequence)
        {
            Calibration();
            Debug.Log("Calibrating...");
        }
        
        // normalize
        floorData = NormalizeFloorData(floorData, calibrationData);
        
        // ignore dead cells
        // IgnoreDeadCells();
        
        // interpolate dead cells
        InterpolateDeadCells();
        
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
            _isCalibrationSequence = false;
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

    private readonly int[][] _deadCellList = new int[][]
    {
        new int[] {0, 3},
        new int[] {2, 1},
        new int[] {3, 2},
        new int[] {4, 0},
        new int[] {5, 2}
    };
    
    private void IgnoreDeadCells()
    {
        for(int i = 0; i < _deadCellList.Length; i++)
        {
            floorData.p[_deadCellList[i][0], _deadCellList[i][1]] = 0;
        }
    }

    private void InterpolateDeadCells()
    {
        for(int i = 0; i < _deadCellList.Length; i++)
        {
            // 上下左右のセルの平均値を計算する
            double sum = 0;
            int count = 0;
            int row = _deadCellList[i][0];
            int col = _deadCellList[i][1];
            
            if(1 < row)
            {
                sum += floorData.p[row-1, col];
                count++;
            }
            
            if(row < Rows-1)
            {
                sum += floorData.p[row+1, col];
                count++;
            }
            
            if(1 < col)
            {
                sum += floorData.p[row, col-1];
                count++;
            }
            
            if(col < Columns-1)
            {
                sum += floorData.p[row, col+1];
                count++;
            }

            if (count > 0)
            {
                floorData.p[row, col] = sum / count;
            }
        }
    }
    
    private void UpdateFloorImageTexture()
    {
        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                floorImageTexture.SetPixel(j, Rows-1-i, new Color((float)floorData.p[i, j], (float)floorData.p[i, j], (float)floorData.p[i, j]));
            }
        }
        
        floorImageTexture.Apply();
    }
}

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

using UnityEngine;
using UnityEngine.Serialization;

public class TVRParser : MonoBehaviour
{
    private TVRSerialHandler _serialHandler;
    public double[,] floorData;
    private int _rows = 6;
    private int _columns = 6;
    
    void Start()
    {
        _serialHandler = FindObjectOfType<TVRSerialHandler>();
    }
    
    void Update()
    {
        if (_serialHandler.message != null)
        {
            floorData = ParseStringToDoubleArray(_serialHandler.message, _rows, _columns);
            SortFloorData();
        }
        
        Debug.Log(floorData[0, 0]);
    }
    
    private double[,] ParseStringToDoubleArray(string data, int rows, int columns)
    { 
        string dataTrim = data.Trim(' ');
        
        // 文字列をカンマで分割して配列にする
        string[] splitData = dataTrim.Split(',');
        
        // 配列の長さを確認
        // Debug.Log(splitData.Length);

        // 配列のサイズが不適切な場合はエラーを返す
        if (splitData.Length != rows * columns)
        {
            Debug.LogError("Data length does not match the specified array size.");
            return null;
        }

        double[,] doubleArray = new double[rows, columns];
        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // 文字列をdoubleに変換して配列に格納
                if (double.TryParse(splitData[index], out double value))
                {
                    doubleArray[row, col] = value;
                }
                else
                {
                    Debug.LogError($"Invalid data format: {splitData[index]}");
                    return null;
                }
                index++;
            }
        }

        return doubleArray;
    }

    private void SortFloorData()
    {
        double[,] sortedFloorData = new double[_rows, _columns];
        for(int i = 0; i < _rows; i++)
        {
            for(int j = 0; j < _columns; j++)
            {
                sortedFloorData[i, j] = floorData[SortMatrix[i, j, 0], SortMatrix[i, j, 1]];
            }
        }
        
        floorData = sortedFloorData;
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
}
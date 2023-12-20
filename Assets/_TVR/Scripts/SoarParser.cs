using UnityEngine;

public class SoarParser : MonoBehaviour
{
    [SerializeField] private SoarSerialHandler serialHandler;
    public FloorData floorData;
    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 6;
    
    void Start()
    {
        if (serialHandler == null)
        {
            serialHandler = FindFirstObjectByType<SoarSerialHandler>();
        }

        floorData = new FloorData();
        floorData.p = new double[rows, columns];
    }
    
    void Update()
    {
        if (serialHandler.message != null)
        {
            floorData.p = ParseStringToDoubleArray(serialHandler.message, rows, columns);
        }
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
            // Debug.LogError("Data length does not match the specified array size.");
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
                    // Debug.LogError($"Invalid data format: {splitData[index]}");
                    return null;
                }
                index++;
            }
        }

        return doubleArray;
    }
}
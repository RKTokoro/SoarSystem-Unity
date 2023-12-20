using System;
using System.IO.Ports;
using UnityEngine;
using UniRx;

// with reference to
// https://tomosoft.jp/design/?p=7256
// https://coworking-nagaokakyo.jp/note/post-3324/
// https://nn-hokuson.hatenablog.com/entry/2017/09/12/192024

public class SoarSerialHandler : MonoBehaviour
{
    [SerializeField] private string portName;
    [SerializeField] private int baudRate = 9600;
    
    private SerialPort _serialPort;
    private bool _isLoop = true;
    
    [HideInInspector] public string message;

    void Start () 
    {
        this._serialPort = new SerialPort (portName, baudRate, Parity.None, 8, StopBits.One);

        try
        {
            this._serialPort.Open();
            // Debug.Log ("open serial port");
            
            Scheduler.ThreadPool.Schedule (() => ReadData ()).AddTo(this);
        } 
        catch(Exception e)
        {
            Debug.Log ("can not open serial port");
        }
    }
	
    private void ReadData()
    {
        int bufferSize = 500; // バッファサイズを500バイトに設定
        byte[] buffer = new byte[bufferSize];

        while (this._isLoop && this._serialPort.IsOpen)
        {
            try
            {
                if (this._serialPort.BytesToRead > 0)
                {
                    int bytesRead = this._serialPort.Read(buffer, 0, buffer.Length);
                    message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // Debug.Log(message);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    void OnDestroy()
    {
        this._isLoop = false;
        this._serialPort.Close ();
    }
}

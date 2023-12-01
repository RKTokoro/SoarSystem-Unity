using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO.Ports;
using UnityEngine;
using UniRx;

// with reference to
// https://tomosoft.jp/design/?p=7256
// https://coworking-nagaokakyo.jp/note/post-3324/
// https://nn-hokuson.hatenablog.com/entry/2017/09/12/192024

public class TVRSerialHandler : MonoBehaviour
{
    public string portName;
    public int baurate = 9600;

    SerialPort serial;
    bool isLoop = true;
    
    [HideInInspector] public string message;

    void Start () 
    {
        this.serial = new SerialPort (portName, baurate, Parity.None, 8, StopBits.One);

        try
        {
            this.serial.Open();
            // Debug.Log ("open serial port");
            
            Scheduler.ThreadPool.Schedule (() => ReadData ()).AddTo(this);
        } 
        catch(Exception e)
        {
            Debug.Log ("can not open serial port");
        }
    }
	
    public void ReadData()
    {
        int bufferSize = 500; // バッファサイズを500バイトに設定
        byte[] buffer = new byte[bufferSize];

        while (this.isLoop && this.serial.IsOpen)
        {
            try
            {
                if (this.serial.BytesToRead > 0)
                {
                    int bytesRead = this.serial.Read(buffer, 0, buffer.Length);
                    string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log(message);
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
        this.isLoop = false;
        this.serial.Close ();
    }
}

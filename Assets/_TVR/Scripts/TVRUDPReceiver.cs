// https://qiita.com/nenjiru/items/d9c4e8a22601deb0425b

using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Serialization;

public class TVRUDPReceiver : MonoBehaviour
{
    [SerializeField] private int localPort = 10712;
    private static UdpClient _udp;
    private Thread _thread;
    [HideInInspector] public string receiveText;

    private void Start ()
    {
        _udp = new UdpClient(localPort);
        _udp.Client.ReceiveTimeout = 1000;
        _thread = new Thread(new ThreadStart(ThreadMethod));
        _thread.Start(); 
    }

    private void OnApplicationQuit()
    {
        _thread.Abort();
    }

    private void ThreadMethod()
    {
        while(true)
        {
            IPEndPoint remoteEp = null;
            byte[] data = _udp.Receive(ref remoteEp);
            string text = Encoding.ASCII.GetString(data);
            // Debug.Log(text);  // checking received text in console
            receiveText = text;
        }
    }
}


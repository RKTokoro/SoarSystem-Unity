using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using UnityEngine.Serialization;

public class DataCollecter : MonoBehaviour
{
    [SerializeField] private Vector3 acceleration = Vector3.forward;
    [SerializeField] private int recordFrames = 300;
    [SerializeField] private int bufferFrames = 5;
    [SerializeField] private int fps = 60;
    [SerializeField] private int imageWidth = 6;
    [SerializeField] private int imageHeight = 6;
    
    [SerializeField] private MLImageGenerator mlImageGenerator;
    private Texture2D _inputTexture;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        if (state == State.Forward)
        {
            acceleration = Vector3.forward;
            _stateSavePath = "01_Forward";
        }
        else if (state == State.Backward)
        {
            acceleration = Vector3.back;
            _stateSavePath = "02_Backward";
        }
        else if (state == State.Left)
        {
            acceleration = Vector3.left;
            _stateSavePath = "03_Left";
        }
        else if (state == State.Right)
        {
            acceleration = Vector3.right;
            _stateSavePath = "04_Right";
        }
        else if (state == State.Up)
        {
            acceleration = Vector3.up;
            _stateSavePath = "05_Up";
        }
        else if (state == State.Down)
        {
            acceleration = Vector3.down;
            _stateSavePath = "06_Down";
        }
        
        Debug.Log("Acceleration: " + acceleration);
        
        if(mlImageGenerator == null)
        {
            mlImageGenerator = FindFirstObjectByType<MLImageGenerator>();
        }
        
        StartNewSession();
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(OVRInput.RawButton.X) || Input.GetKeyDown(KeyCode.Space))
        {
            // CollectDataAsync(recordFrames, fps, acceleration);
            StartCoroutine(CollectDataCoroutine(recordFrames, bufferFrames, fps, acceleration));
            Debug.Log("Start recording.");
        }
    }
    
    // ------------------------------
    // merge imagerecoeder and automover into this class

    [SerializeField] private TVRFloorDataManager floorDataManager;
    [SerializeField] private TVRSoarBoard soarBoard;
    
    private double[,] _floorDataArray = new double[6, 6];
    [SerializeField] private string baseSavePath = "RecordData/v1217"; 
    private string _stateSavePath;
    private string _currentSessionPath;
    private string _timestamp;

    private void StartNewSession()
    {
        _timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        string relativePath = Path.Combine(baseSavePath, _stateSavePath);
        _currentSessionPath = Path.Combine(Application.dataPath, relativePath);

        if (!Directory.Exists(_currentSessionPath))
        {
            Directory.CreateDirectory(_currentSessionPath);
            Debug.Log("Created new session directory at " + _currentSessionPath);
        }
        else
        {
            Debug.Log("Session directory already exists at " + _currentSessionPath);
        }
    }
    
    private IEnumerator CollectDataCoroutine(int recordFrameCount, int bufferFrameCount, int fps, Vector3 a)
    {
        // StartNewSession();
        int frameCount = 0;
        while (frameCount < recordFrameCount)
        {
            soarBoard.a = a;
            if (frameCount >= bufferFrameCount)
            {
                UpdateArray();
                SaveArrayAsImage(frameCount);
                Debug.Log("Saved image at " + frameCount.ToString("D3") + " frame.");
            }

            if (frameCount % 5 == 0)
            {
                if (frameCount % (5 * 4) == 0)
                {
                    PlayRhythmSound(rhythmSound01);
                }
                else
                {
                    PlayRhythmSound(rhythmSound02);
                }
            }
            
            frameCount++;
            yield return new WaitForSeconds(1.0f / fps);
        }
        soarBoard.a = Vector3.zero;
        soarBoard.Reset();
        Debug.Log("Finished recording.");
    }
    
    void UpdateArray()
    {
        if (floorDataManager.floorData != null)
        {
            _floorDataArray = floorDataManager.floorData.p;
            // Debug.Log("array updated.");
        }
        else
        {
            // Debug.LogWarning("Floor data is null.");
        }
    }

    void SaveArrayAsImage(int frameNumber)
    {
        _inputTexture = mlImageGenerator.inputTexture;
        
        byte[] bytes = _inputTexture.EncodeToPNG();
        
        string filepath = Path.Combine(_currentSessionPath, _timestamp + "_frame_" + frameNumber.ToString("D3") + ".png");
        File.WriteAllBytes(filepath, bytes);
    } 
    
    // audio
    // ------------------------------
    
    [SerializeField] private AudioClip rhythmSound01;
    [SerializeField] private AudioClip rhythmSound02;
    private AudioSource _audioSource;
    
    private void PlayRhythmSound(AudioClip clip)
    {
        _audioSource.clip = clip;
        float soundLength = clip.length;
        Coroutine playSoundCoroutine = StartCoroutine(PlaySoundCoroutine(soundLength));
        StopCoroutine(playSoundCoroutine);
    }

    IEnumerator PlaySoundCoroutine(float length)
    {
        _audioSource.Play();
        yield return new WaitForSeconds(length);
        _audioSource.Stop();
        _audioSource.time = 0;
    }
    
    // ------------------------------
    
    // enum
    // ------------------------------
    public enum State
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }
    
    public State state = State.Forward;
}

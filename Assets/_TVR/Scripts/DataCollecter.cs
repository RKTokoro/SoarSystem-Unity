using System;
using System.Threading.Tasks;
using UnityEngine;

public class DataCollecter : MonoBehaviour
{
    private ImageRecorder _imageRecorder;
    [SerializeField] private GameObject _collectBoard;
    private Animator _boardAnimator;
    
    // Start is called before the first frame update
    void Start()
    {
        _imageRecorder = FindObjectOfType<ImageRecorder>();
        _boardAnimator = _collectBoard.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(OVRInput.RawButton.X))
        {
            if (_imageRecorder != null)
            {
                _imageRecorder.StartRecording();
            }
            Debug.Log("Start recording.");

            if (_boardAnimator != null)
            {
                _boardAnimator.Play("MoveForward");
            }
        }
    }
    
    async Task StartRecording()
    {
        while (true) {
            await Task.Delay(TimeSpan.FromSeconds(1.0 / 60));
            // メインスレッドで変数を保存する処理を記述
            
        }
    }
}

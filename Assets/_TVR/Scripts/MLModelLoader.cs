using UnityEngine;
using Unity.Sentis;

public class MLModelLoader : MonoBehaviour
{
    public Texture2D inputTexture;
    public ModelAsset modelAsset;

    private Tensor _inputTensor;
    private TensorFloat _outputTensor;
    private Model _runtimeModel;
    private IWorker _worker;
    public float[] results;
    
    void Start()
    {
        // Create the runtime model
        _runtimeModel = ModelLoader.Load(modelAsset);
        
        // Create an engine
        _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, _runtimeModel);
    }

    private void Update()
    {
        EstimateState();
    }

    private void OnDisable()
    {
        // Tell the GPU we're finished with the memory the engine used 
        _worker.Dispose();
    }

    private void EstimateState()
    {
        // Create input data as a tensor
        _inputTensor = TextureConverter.ToTensor(inputTexture, width:6, height:6, channels:1);
        
        // Run the model with the input data
        _worker.Execute(_inputTensor);
        
        // Get the result
        _outputTensor = _worker.PeekOutput() as TensorFloat;

        if (_outputTensor != null)
        {
            _outputTensor.MakeReadable();
            results = _outputTensor.ToReadOnlyArray();
        }
    }
}
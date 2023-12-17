using UnityEngine;
using Unity.Sentis;
using System.Linq;

public class MLModelLoader : MonoBehaviour
{
    public Texture2D inputTexture;
    public ModelAsset modelAsset;

    private Tensor _inputTensor;
    private TensorFloat _outputTensor;
    private Model _runtimeModel;
    private IWorker _worker;
    public float[] results;
    public int estimatedResult;

    [SerializeField] private TVRFloorDataManager floorDataManager;
    [SerializeField] private MLImageGenerator mlImageGenerator;
    
    void Start()
    {
        // Create the runtime model
        _runtimeModel = ModelLoader.Load(modelAsset);
        // Create an engine
        _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, _runtimeModel);
        
        if(floorDataManager == null)
        {
            floorDataManager = FindFirstObjectByType<TVRFloorDataManager>();
        }
        
        if(mlImageGenerator == null)
        {
            mlImageGenerator = FindFirstObjectByType<MLImageGenerator>();
        }
    }

    private void Update()
    {
        inputTexture = mlImageGenerator.inputTexture;
        EstimateResult();
    }

    private void OnDisable()
    {
        // Tell the GPU we're finished with the memory the engine used 
        _worker.Dispose();
    }

    private void EstimateResult()
    {
        // Create input data as a tensor
        _inputTensor = TextureConverter.ToTensor(inputTexture, width:9, height:6, channels:1);
        
        // Run the model with the input data
        _worker.Execute(_inputTensor);
        
        // Get the result
        _outputTensor = _worker.PeekOutput() as TensorFloat;

        if (_outputTensor != null)
        {
            _outputTensor.MakeReadable();
            results = _outputTensor.ToReadOnlyArray();
            estimatedResult = System.Array.IndexOf(results, results.Max());
        }
    }
}
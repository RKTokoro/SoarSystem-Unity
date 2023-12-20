using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoarHUD : MonoBehaviour
{
    private SoarBoard _soarBoard;
    private GameObject _head;
    
    [SerializeField] private GameObject heightIndicator;
    [SerializeField] private GameObject ascendIndicator;
    [SerializeField] private GameObject descendIndicator;
    
    private RectTransform _heightIndicatorRectTransform;
    private RectTransform _ascendIndicatorRectTransform;
    private RectTransform _descendIndicatorRectTransform;
    [SerializeField] private RawImage inputImageRawImage;
    [SerializeField] private MLImageGenerator _mlImageGenerator;

    [SerializeField] private MLModelLoader modelLoader;
    [SerializeField] private TextMeshProUGUI stateText;
    
    private Vector3 _heightIndicatorOrigin;
    [SerializeField] private float heightIndicatorScale = 1.0f;
    
    [SerializeField] private RectTransform reticleRectTransform;
    
    private float _HUDDistance = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _head = GameObject.Find("CenterEyeAnchor");
        _soarBoard = FindObjectOfType<SoarBoard>();
        _heightIndicatorRectTransform = heightIndicator.GetComponent<RectTransform>();
        _ascendIndicatorRectTransform = ascendIndicator.GetComponent<RectTransform>();
        _descendIndicatorRectTransform = descendIndicator.GetComponent<RectTransform>();
        
        _heightIndicatorOrigin = _heightIndicatorRectTransform.localPosition;
        
        if(_mlImageGenerator == null)
        {
            _mlImageGenerator = FindFirstObjectByType<MLImageGenerator>();
        }

        if (modelLoader == null)
        {
            modelLoader = FindFirstObjectByType<MLModelLoader>();
        }

        _HUDDistance = this.GetComponent<Canvas>().planeDistance;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHeightIndicator();
        UpdateInputImage();
        UpdateStateText();
        UpdateReticle();
    }
    
    private void UpdateHeightIndicator()
    {
        float height = _head.transform.localPosition.y;
        
        _heightIndicatorRectTransform.localPosition =
            _heightIndicatorOrigin + heightIndicatorScale * height * Vector3.up;
        
        _ascendIndicatorRectTransform.localPosition =
            _heightIndicatorOrigin + heightIndicatorScale * (_soarBoard.headHeightDefault + _soarBoard.ascendThreshold) * Vector3.up;
        _descendIndicatorRectTransform.localPosition =
            _heightIndicatorOrigin + heightIndicatorScale * (_soarBoard.headHeightDefault - _soarBoard.descendThreshold) * Vector3.up;
    }
    
    private void UpdateInputImage()
    {
        inputImageRawImage.texture = _mlImageGenerator.inputTexture;
    }

    private void UpdateStateText()
    {
        stateText.text = modelLoader.estimatedResult.ToString();
    }

    private void UpdateReticle()
    {
        // roll
        reticleRectTransform.localRotation = Quaternion.Euler(
            new Vector3(0.0f, 0.0f, _head.transform.localRotation.eulerAngles.z - _soarBoard.headRotationDefault.z));
        
        // pitch
        reticleRectTransform.localPosition = new Vector3(
            0,
            -Mathf.Sin(Mathf.Deg2Rad * (_head.transform.localRotation.eulerAngles.x - _soarBoard.headRotationDefault.x)) * _HUDDistance * 20.0f,
            0);
    }
}

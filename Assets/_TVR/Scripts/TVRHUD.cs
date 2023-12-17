using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TVRHUD : MonoBehaviour
{
    private TVRSoarBoard _soarBoard;
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
    
    // Start is called before the first frame update
    void Start()
    {
        _head = GameObject.Find("CenterEyeAnchor");
        _soarBoard = FindObjectOfType<TVRSoarBoard>();
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
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHeightIndicator();
        UpdateInputImage();
        UpdateStateText();
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
}

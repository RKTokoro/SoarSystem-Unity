using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class MLImageGenerator : MonoBehaviour
{
    [SerializeField] private TVRFloorDataManager floorDataManager;
    [SerializeField] private GameObject centerEyeAnchor, leftHandAnchor, rightHandAnchor;
    private Transform _headTransform, _leftHandTransform, _rightHandTransform;
    [SerializeField] private float heightRange = 2.0f;
    [SerializeField] private float widthRange = 2.0f;
    
    [HideInInspector] public Texture2D inputTexture;
    
    // Start is called before the first frame update
    void Start()
    {
        if(floorDataManager == null)
        {
            floorDataManager = FindFirstObjectByType<TVRFloorDataManager>();
        }

        _headTransform = centerEyeAnchor.GetComponent<Transform>();
        _leftHandTransform = leftHandAnchor.GetComponent<Transform>();
        _rightHandTransform = rightHandAnchor.GetComponent<Transform>();
        
        inputTexture = new Texture2D(TVRFloorDataManager.Rows+3, TVRFloorDataManager.Columns, TextureFormat.R16, false);
        inputTexture.filterMode = FilterMode.Point;
    }
    
    // Update is called once per frame
    void Update()
    {
        GenerateImage();
    }

    void GenerateImage()
    {
        for(int i = 0; i < TVRFloorDataManager.Rows; i++)
        {
            for(int j = 0; j < TVRFloorDataManager.Columns; j++)
            {
                inputTexture.SetPixel(j, TVRFloorDataManager.Rows-1-i, 
                    new Color(
                    (float)floorDataManager.floorData.p[i, j], 
                    (float)floorDataManager.floorData.p[i, j], 
                    (float)floorDataManager.floorData.p[i, j]
                    ));
            }
        }
        
        SetTransformToTexture(_headTransform, TVRFloorDataManager.Rows);
        SetTransformToTexture(_leftHandTransform, TVRFloorDataManager.Rows + 1);
        SetTransformToTexture(_rightHandTransform, TVRFloorDataManager.Rows + 2);
        
        inputTexture.Apply();
    }
    
    void SetTransformToTexture(Transform transform, int row)
    {
        Vector3 position = NormalizePosition(transform.localPosition);
        Vector3 rotation = NormalizeRotation(transform.localRotation.eulerAngles);

        // Textureに位置情報をセット
        inputTexture.SetPixel(row, 5, new Color(position.x, position.x, position.x));
        inputTexture.SetPixel(row, 4, new Color(position.y, position.y, position.y));
        inputTexture.SetPixel(row, 3, new Color(position.z, position.z, position.z));

        // Textureに回転情報をセット
        inputTexture.SetPixel(row, 2, new Color(rotation.x, rotation.x, rotation.x));
        inputTexture.SetPixel(row, 1, new Color(rotation.y, rotation.y, rotation.y));
        inputTexture.SetPixel(row, 0, new Color(rotation.z, rotation.z, rotation.z));
    }

    Vector3 NormalizePosition(Vector3 position)
    {
        position.x = (position.x + 1.0f) * 0.5f;
        position.y = position.y * 0.5f;
        position.z = (position.z + 1.0f) * 0.5f;
        return position;
    }

    Vector3 NormalizeRotation(Vector3 rotation)
    {
        rotation.x = ((rotation.x + 360) % 360) / 360.0f;
        rotation.y = ((rotation.y + 360) % 360) / 360.0f;
        rotation.z = ((rotation.z + 360) % 360) / 360.0f;
        return rotation;
    }
}

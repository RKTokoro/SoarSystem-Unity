using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVRFloor : MonoBehaviour
{
    private TVRParser _parser;
    private double[,] _floorData;

    private GameObject[,] _debugFloorTiles;
    private Material[,] _debugFloorMaterials;
    
    private bool _floorIsSpawned = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _parser = FindObjectOfType<TVRParser>();
    }
    
    

    // Update is called once per frame
    void Update()
    {
        // update floor data
        _floorData = _parser.floorData;
        
        if(_floorData != null && !_floorIsSpawned)
        {
            SpawnFloor();
            _floorIsSpawned = true;
        }
        
        if(_floorData != null && _floorIsSpawned)
        {
            UpdateFloorColor();
        }
    }
    
    private void SpawnFloor()
    {
        _debugFloorTiles = new GameObject[_floorData.GetLength(0),_floorData.GetLength(1)];
        _debugFloorMaterials = new Material[_floorData.GetLength(0),_floorData.GetLength(1)];
        
        for (int i = 0; i < _floorData.GetLength(0); i++)
        {
            for (int j = 0; j < _floorData.GetLength(1); j++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = new Vector3(i, j, 0);
                tile.transform.parent = this.transform;
                tile.name = $"FloorTile_{i}_{j}";
                
                // add
                _debugFloorTiles[i, j] = tile;
                
                Material tileMaterial = tile.GetComponent<Renderer>().material;
                // Transparentに変更
                tileMaterial.shader = Shader.Find("Unlit/Color");
                tileMaterial.SetFloat("_Mode", 2);
                tileMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                tileMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                tileMaterial.SetInt("_ZWrite", 0);
                tileMaterial.DisableKeyword("_ALPHATEST_ON");
                tileMaterial.EnableKeyword("_ALPHABLEND_ON");
                tileMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                tileMaterial.renderQueue = 3000;
                // マテリアルのプロパティを更新
                tileMaterial.SetOverrideTag("RenderType", "Transparent");
                tileMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                tileMaterial.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                
                // add
                _debugFloorMaterials[i, j] = tileMaterial;
            }
        }
        
        _floorIsSpawned = true;
    }
    
    private void UpdateFloorColor()
    {
        for (int i = 0; i < _floorData.GetLength(0); i++)
        {
            for (int j = 0; j < _floorData.GetLength(1); j++)
            {
                // update tile color
                _debugFloorMaterials[i, j].color = 
                    Color.HSVToRGB(
                        0.0f, 
                        (float)(_floorData[i, j] / 100000.0), 
                        0.8f
                        );
            }
        }
    }
}

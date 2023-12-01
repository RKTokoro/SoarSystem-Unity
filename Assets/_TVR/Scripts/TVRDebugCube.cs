using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TVRDebugCube : MonoBehaviour
{
    // グリッドのサイズを設定
    public int gridX = 5;
    public int gridY = 5;
    public int gridZ = 5;
    
    // キューブ間の間隔
    public float spacing = 2.0f;
    
    public float size = 0.1f;
    
    [Range(0.0f, 1.0f)]public float debugCubeAlpha = 0.2f;
    
    // Start is called before the first frame update
    void Start()
    {
        SpawnDebugCubes();
    }

    void SpawnDebugCubes()
    {
        
        // グリッドの中心を計算
        Vector3 gridCenter = 
            new Vector3(
                (gridX - 1) * spacing / 2,
                (gridY - 1) * spacing / 2, 
                (gridZ - 1) * spacing / 2
                );

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                for (int z = 0; z < gridZ; z++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    
                    // delete collider
                    Destroy(cube.GetComponent<BoxCollider>());

                    // キューブの位置を計算し、グリッドの中心を原点に調整
                    float posX = x * spacing - gridCenter.x;
                    float posY = y * spacing - gridCenter.y;
                    float posZ = z * spacing - gridCenter.z;
                    cube.transform.position = new Vector3(posX, posY, posZ);
                    
                    // キューブのサイズを設定
                    cube.transform.localScale = new Vector3(size, size, size);

                    Material cubeMaterial = cube.GetComponent<Renderer>().material;
                    
                    // use transparent shader
                    cube.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
                    
                    // change surface type to transparent
                    cubeMaterial.SetFloat("_Surface", 1.0f);
                    cubeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    cubeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    cubeMaterial.SetInt("_ZWrite", 0);
                    cubeMaterial.DisableKeyword("_ALPHATEST_ON");
                    cubeMaterial.EnableKeyword("_ALPHABLEND_ON");
                    cubeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    cubeMaterial.renderQueue = 3000;
                    
                    // キューブの色を設定
                    cubeMaterial.color = new Color(
                        (float)x / gridX,
                        (float)y / gridY,
                        (float)z / gridZ,
                        debugCubeAlpha
                        );
                    
                    // キューブの親を設定
                    cube.transform.parent = this.transform;
                    
                    // キューブの名前を設定
                    cube.name = "Cube_" + x + "_" + y + "_" + z;
                }
            }
        }
    }
}

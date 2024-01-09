using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoarShot : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    public bool isShot = false;
    public float shotInterval = 0.1f;
    public GameObject target;
    
    Coroutine _shotCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isShot)
        {
            if (_shotCoroutine == null)
            {
                _shotCoroutine = StartCoroutine(Shot());
            }
        }
        else
        {
            if(_shotCoroutine != null)
            {
                StopCoroutine(_shotCoroutine);
            }
            _shotCoroutine = null;
        }
    }
    
    private IEnumerator Shot()
    {
        while (true)
        {
            GameObject newBulletGameObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            SoarBullet newBullet = newBulletGameObject.GetComponent<SoarBullet>();
            
            // bulletに初期値を設定する
            newBullet.target = target;
            
            // wait for shotInterval
            yield return new WaitForSeconds(shotInterval);
        }
    }
}

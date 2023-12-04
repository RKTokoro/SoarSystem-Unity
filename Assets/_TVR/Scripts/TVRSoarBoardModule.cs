using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVRSoarBoardModule : MonoBehaviour
{
    private TVRSoarBoard _soarBoard;
    
    private ParticleSystem[] _particleSystems;
    public enum ModuleType
    {
        FL,
        FR,
        BL,
        BR
    }
    
    public ModuleType moduleType;
    
    // Start is called before the first frame update
    void Start()
    {
        _soarBoard = GetComponentInParent<TVRSoarBoard>();
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (moduleType == ModuleType.FL)
        {
            ControlParticle(_soarBoard._pressuresNormalized[0, 0]);
        }else if (moduleType == ModuleType.FR)
        {
            ControlParticle(_soarBoard._pressuresNormalized[0, 1]);
        }else if (moduleType == ModuleType.BL)
        {
            ControlParticle(_soarBoard._pressuresNormalized[1, 0]);
        }else if (moduleType == ModuleType.BR)
        {
            ControlParticle(_soarBoard._pressuresNormalized[1, 1]);
        }
    }
    
    private void ControlParticle(double pressure)
    {
        foreach (var particleSystem in _particleSystems)
        {
            var emission = particleSystem.emission;
            emission.rateOverTime = (float) pressure * 20;
        }
    }
}

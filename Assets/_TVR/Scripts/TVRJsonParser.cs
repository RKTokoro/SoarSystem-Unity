using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// https://qiita.com/rawr/items/c70b054a98a72e3f49f5
public class FfData
{
    [JsonProperty("epoch")]
    public long epoch { get; set; }

    [JsonProperty("floor")]
    public double[,] floor { get; set; }
    // sometimes it is int, sometimes it is double. I don't know why.
}

public class FfHuman
{
    [JsonProperty("id")]
    public int id;
    
    [JsonProperty("human")]
    public FfHumanData human;
}

public class FfHumanData
{
    [JsonProperty("peaks")]
    public FfPeak[] peaks { get; set; }
    
    [JsonProperty("cog")]
    public FfPointFloat cog { get; set; }
    
    [JsonProperty("weight")]
    public double weight { get; set; }
}

public class FfPeak
{
    [JsonProperty("index")]
    public FfPointInt index { get; set; }
    
    [JsonProperty("sum")]
    public double sum { get; set; }
    
    [JsonProperty("cog")]
    public FfPointFloat cog { get; set; }
}

public class FfPointInt
{
    public int x { get; set; }
    public int y { get; set; }
}

public class FfPointFloat
{
    public float x { get; set; }
    public float y { get; set; }
}

public class TVRJsonParser : MonoBehaviour
{
    [SerializeField] private GameObject receiver; 
    private TVRUDPReceiver _udp;
    private string _rawJsonText;
    public FfData ffData;
    public FfHuman[] humanList;

    // Start is called before the first frame update
    private void Start()
    {
        _udp = receiver.GetComponent<TVRUDPReceiver>();
    }

    // Update is called once per frame
    private void Update()
    {
        // update rawJsonText
        _rawJsonText = _udp.receiveText;
        // Debug.Log(_rawJsonText);
        
        // convert process
        TVRJsonConvert(_rawJsonText);
    }
    
    // humane
    private void TVRJsonConvert(string jsonText)
    {
        // _rawJsonText = _udp.receiveText;
        if (jsonText.Contains("floor"))
        {
            ffData = JsonConvert.DeserializeObject<FfData>(jsonText);
            Debug.Log("FfDataのデシリアライズ成功");
        }
        else
        {
            Debug.Log(jsonText);
            humanList = JsonConvert.DeserializeObject<FfHuman[]>(jsonText);
            Debug.Log("FfHumanListのデシリアライズ成功");
        }
    }
}
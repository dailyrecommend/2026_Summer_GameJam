using UnityEngine;

[System.Serializable]
public class cardWeightData
{
    // 다빈치
    public float special;
    public float notSpecialOnly2;

    // 우노
    public float plusTwo;
    public float notSpecialOnlyReverse;

    // 뱅
    public float miss;
    public float BAndBStandardBang;

    // 공통
    public float playerNot1Enemy6;
}

public class AIDataManager : MonoBehaviour
{
    public static AIDataManager Instance 
    { 
        get;    
        private set; 
    }

    public cardWeightData data;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DataLoad();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void DataLoad()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("CardWeight");
        data = JsonUtility.FromJson<cardWeightData>(jsonAsset.text);
    }
}

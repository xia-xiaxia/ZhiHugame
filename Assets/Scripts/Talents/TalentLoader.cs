using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentLoader : MonoBehaviour
{
    public TextAsset jsonFile;
    public List<Talent> talents = new List<Talent>();
    void Start()
    {
        LoadTalents();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            LoadTalents();
            LoadDebugTalents();
        }
    }

    void LoadTalents()
    {
        if (jsonFile != null)
        {
            TalentData talentData = JsonUtility.FromJson<TalentData>(jsonFile.text);
            Debug.Log("Loaded " + talentData.talentList.Count + " talents.");
            foreach (var talent in talentData.talentList)
            {
                talents.Add(talent);
            }
        }
    }

    void LoadDebugTalents()
    {
        var onetalent = talents[0];
        Debug.Log("Debug Talent: " + onetalent.name + " - " + onetalent.description + " Cost: " + onetalent.cost + " kingLimitChange: " + onetalent.talentEffect.kingLimitChange);
    }   

}

[System.Serializable]
public class TalentData
{
    public List<Talent> talentList = new List<Talent>();
}

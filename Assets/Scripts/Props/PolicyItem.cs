using System.Collections.Generic;

[System.Serializable]
public class PolicyItem
{
    public string id; // 道具ID
    public int type; // 1阈值 2免死 3跳过 4调控
    public int thresholdDelta; // 阈值变化（仅阈值道具用）
    public List<int> deathImmunity; // 免死情况（如 1国君 2卿士 3贵族 4外臣 5庶人 6事件杀）
    public int kingChange, nobleChange, scholarChange, foreignChange, peopleChange; // 五大数值变化
    public int usageCount; // 可用次数（-1无限，0销毁）
    public string name; // 名称
    public string desc; // 说明

}

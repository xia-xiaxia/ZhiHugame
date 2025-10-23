using System.Collections.Generic;

[System.Serializable]
public class PolicyItem
{
    public string id; // 道具ID
    public int type; // 1阈值 2免死 3跳过 4调控
    public string name; // 名称
    public string desc; // 说明
    public string result; // 效果
    public string whichChange; // 哪个数值的阈值变化（如 "king" "noble" "scholar" "foreign" "people"）
    public int thresholdDeltaup; // 阈值变化上限（仅阈值道具用）
    public int thresholdDeltadown; // 阈值变化下限（仅阈值道具用）
    public List<int> deathImmunity; // 免死情况（如 1国君上限 -1国君下限 2卿士上限 -2卿士下限 3贵族上限 -3贵族下限 4外臣上限 -4外臣下限 5庶人上限 -5庶人下限 6事件杀）
    public int kingChange, nobleChange, scholarChange, foreignChange, peopleChange; // 五大数值变化
    public int usageCount; // 可用次数（-1无限，0销毁）
    public int cost; // 道具价格
    public string deathdec;  // 免死道具生效文案

}

# UIManager 重构完成说明

## 📦 重构概览

原 `UIManager.cs`（1502行）已成功拆分为 **7个独立模块** + 1个协调器：

### 新增文件
1. **EventDisplayUI.cs** - 事件显示UI（约350行）
2. **StatsDisplayUI.cs** - 数值显示UI（约70行）
3. **EndingUI.cs** - 结局面板UI（约150行）
4. **PolicyMenuUI.cs** - 道具菜单UI（约250行）
5. **PolicyShopUI.cs** - 道具商店UI（约250行）
6. **BuffUI.cs** - BUFF面板UI（约200行）
7. **DeathImmunityUI.cs** - 免死道具UI（约100行）
8. **UIManager.cs** - 协调器（约250行，原1502行）

### 备份文件
- **UIManager_Backup.cs** - 原始完整版本（已备份）

---

## 🔧 Unity Inspector 配置指南

### 步骤 1：创建 UI 管理器对象

在场景中找到或创建以下 GameObject：

1. **EventDisplayUI**
   - 将 `EventDisplayUI.cs` 脚本添加到场景中的一个 GameObject
   - 在 Inspector 中拖入以下组件：
     - `titleText` - 事件标题
     - `speakerName` - 说话人名字
     - `dialoguePanel` - 对话面板
     - `optionButtons[4]` - 4个选项按钮
     - `nextSentenceButton` - 下一句按钮
     - `autoPlayButton` - 自动播放按钮

2. **StatsDisplayUI**
   - 将 `StatsDisplayUI.cs` 添加到场景
   - 拖入：
     - `statText1` - 国君数值
     - `statText2` - 贵族数值
     - `statText3` - 学者数值
     - `statText4` - 外交数值
     - `statText5` - 民心数值
     - `currentYearText` - 年份显示
     - `stats` - StatModel ScriptableObject

3. **EndingUI**
   - 拖入：
     - `endingPanel` - 结局面板GameObject
     - `endingText` - 结局描述文字
     - `endingImage` - 结局图片
     - `endingImageBottom` - 结局图片底图
     - `endingYearText` - 存活年数显示
     - `restartButton` - 重开按钮

4. **PolicyMenuUI**
   - 拖入：
     - `policyMenuPanel` - 道具菜单面板
     - `policyItemsParent` - 道具列表容器
     - `policyItemButtonPrefab` - 道具按钮预制体
     - `stats` - StatModel

5. **PolicyShopUI**
   - 拖入：
     - `policyShopPanel` - 商店面板
     - `shopItemsParent` - 商店道具容器
     - `shopItemButtonPrefab` - 商店道具预制体
     - `currencyText` - 货币显示
     - `restartGameButton` - 重开按钮
     - `backToMenuButton` - 返回主菜单按钮
     - `shopInventoryParent` - 背包容器
     - `shopInventoryItemPrefab` - 背包道具预制体
     - `shopInventoryCountText` - 背包数量显示

6. **BuffUI**
   - 拖入：
     - `buffPanel` - BUFF面板
     - `buffItemsParent` - BUFF列表容器
     - `buffItemButtonPrefab` - BUFF按钮预制体
     - `buffDetailText` - BUFF详情文字
     - `buffCloseButton` - 关闭按钮
     - `buffOpenButton` - 打开按钮

7. **DeathImmunityUI**
   - 拖入：
     - `deathImmunityPanel` - 免死确认面板
     - `deathImmunityText` - 提示文字
     - `useDeathImmunityButton` - 使用按钮
     - `declineDeathImmunityButton` - 拒绝按钮
     - `deathImmunityMessagePanel` - 生效提示面板
     - `deathImmunityMessageText` - 生效文字
     - `deathImmunityMessageConfirmButton` - 确认按钮

8. **UIManager**（协调器）
   - 拖入：
     - `jinYan` - 主UI容器
     - `stats` - StatModel
     - `exitToMenuButton` - 退出按钮
     - `policyTooltipPanel` - 道具提示框
     - `policyTooltipText` - 提示文字

---

## 🎯 重要提示

### 1. 场景组织建议
推荐创建以下层级结构：
```
Canvas
├── UIManager (UIManager.cs)
├── EventDisplay (EventDisplayUI.cs)
├── StatsDisplay (StatsDisplayUI.cs)
├── EndingPanel (EndingUI.cs)
├── PolicyMenu (PolicyMenuUI.cs)
├── PolicyShop (PolicyShopUI.cs)
├── BuffPanel (BuffUI.cs)
└── DeathImmunityPanel (DeathImmunityUI.cs)
```

### 2. 向后兼容性
新的 `UIManager.cs` 保留了所有外部调用的公共方法，如：
- `ShowEvent(string id)`
- `UpdateStatText()`
- `ShowEndingPanel(...)`
- `ShowPolicyMenu()`
- 等等

**现有代码无需修改**，UIManager 会自动将调用转发到对应的子系统。

### 3. 单例模式
所有UI子系统都使用单例模式：
- `EventDisplayUI.Instance`
- `StatsDisplayUI.Instance`
- `EndingUI.Instance`
- 等等

### 4. 优势
✅ **代码可维护性**：每个类职责单一，易于理解和修改  
✅ **测试友好**：可以独立测试每个UI模块  
✅ **复用性强**：UI组件可以在不同场景中复用  
✅ **团队协作**：不同成员可以并行修改不同UI模块  
✅ **性能优化**：按需加载和更新UI  

---

## 🔄 如果需要回滚

如果遇到问题需要回滚到旧版本：

```powershell
cd "d:\Work\unityChina\Game\DaZhouProject\zhihuProjecttttt-main\zhihuProjecttttt-main\Assets\Scripts\UI"
Remove-Item "UIManager.cs"
Copy-Item "UIManager_Backup.cs" "UIManager.cs"
```

---

## 📝 下一步工作

1. **在 Unity 中打开项目**
2. **创建上述 7 个 GameObject**（或使用现有的）
3. **将对应脚本添加到各个 GameObject**
4. **在 Inspector 中拖入所有引用**
5. **测试各个功能**：
   - 事件显示
   - 道具使用
   - 商店购买
   - BUFF显示
   - 结局显示
   - 免死道具

6. **如有问题，检查 Console 日志**

---

## 💡 常见问题

**Q: 为什么有些按钮点击没反应？**  
A: 检查对应的 UI 脚本是否正确绑定了按钮引用。

**Q: NullReferenceException 错误**  
A: 检查 Inspector 中是否所有必需的引用都已拖入。

**Q: 道具/BUFF 不显示**  
A: 检查 Prefab 是否正确设置，以及 Parent 容器是否正确。

**Q: 如何调试？**  
A: 每个类都有详细的 Debug.Log，打开 Console 查看日志。

---

重构完成！🎉

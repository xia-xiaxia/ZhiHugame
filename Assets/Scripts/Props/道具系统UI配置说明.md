# 道具系统 UI 配置说明

## 一、道具菜单弹窗（主动使用）

### 需要在 UIManager 组件中配置的字段：

1. **Policy Menu Panel** (GameObject)
   - 道具菜单的主弹窗面板
   - 建议包含：标题、关闭按钮、道具列表滚动区域

2. **Policy Items Parent** (Transform)
   - 道具按钮的父容器（通常是 ScrollView 的 Content）
   - 道具按钮会动态生成在这个容器下

3. **Policy Item Button Prefab** (GameObject)
   - 单个道具按钮的预制体
   - 需要包含：
     - Button 组件
     - Text 组件（用于显示道具信息）
   - 文本格式：`{道具名称}\n类型：{类型}\n次数：{次数}\n{描述}`

### 使用方式：
- 在游戏界面添加一个"道具"或"国策"按钮
- 按钮点击调用：`UIManager.Instance.ShowPolicyMenu()`
- 弹窗中会显示所有拥有的道具
- 点击道具按钮即可使用（阈值和免死道具不可点击，显示为灰色）

---

## 二、免死道具确认弹窗（自动触发）

### 需要在 UIManager 组件中配置的字段：

1. **Death Immunity Panel** (GameObject)
   - 免死道具确认弹窗的主面板
   - 当玩家数值超阈值且有免死道具时自动弹出

2. **Death Immunity Text** (Text)
   - 显示道具信息和危机提示的文本框
   - 格式：`检测到致命危机：{危机类型}\n\n是否使用国策：{道具名}？\n{描述}\n剩余使用次数：{次数}`

3. **Use Death Immunity Button** (Button)
   - "使用"按钮
   - 点击后消耗道具并恢复数值到选项点击前

4. **Decline Death Immunity Button** (Button)
   - "不使用"按钮
   - 点击后不消耗道具，继续触发游戏结局

### 触发机制：
- 完全自动，无需手动调用
- 当玩家选择选项后数值超过阈值时自动检测
- 如果有对应的免死道具，会暂停游戏并弹窗询问

---

## 三、道具类型说明

### 1. 阈值道具 (type=1)
- **效果**：游戏开始时自动生效，调整死亡阈值
- **使用**：被动生效，不可主动使用
- **显示**：道具菜单中显示为灰色（不可点击）

### 2. 免死道具 (type=2)
- **效果**：死亡时自动检测并弹窗询问是否使用
- **使用**：自动触发弹窗，玩家选择是否使用
- **恢复**：数值恢复到点击选项前的状态
- **显示**：道具菜单中显示为灰色（不可点击）

### 3. 跳过道具 (type=3)
- **效果**：跳过当前事件，不改变数值，直接抽取新事件
- **使用**：在道具菜单中主动点击使用
- **显示**：道具菜单中可点击

### 4. 调控道具 (type=4)
- **效果**：立即调整五大数值（根据道具配置的 kingChange 等字段）
- **使用**：在道具菜单中主动点击使用
- **显示**：道具菜单中可点击

---

## 四、快速配置步骤

### 1. 创建道具菜单弹窗
```
Canvas
└── PolicyMenuPanel (Panel)
    ├── Title (Text): "国策"
    ├── CloseButton (Button)
    └── ScrollView
        └── Content (Transform) ← 拖到 Policy Items Parent
```

### 2. 创建道具按钮预制体
```
PolicyItemButton (Prefab)
├── Button (Component)
└── Text (Component) ← 显示道具信息
```

### 3. 创建免死道具弹窗
```
Canvas
└── DeathImmunityPanel (Panel)
    ├── PromptText (Text) ← 拖到 Death Immunity Text
    ├── UseButton (Button) ← 拖到 Use Death Immunity Button
    └── DeclineButton (Button) ← 拖到 Decline Death Immunity Button
```

### 4. 绑定到 UIManager
- 在场景中找到 UIManager 对象
- 将上述创建的对象拖到对应的字段
- 确保所有字段都已正确绑定

---

## 五、调用示例

### 在游戏界面添加"道具"按钮
```csharp
// 示例：在某个按钮的 OnClick 事件中
public void OnPolicyButtonClick()
{
    UIManager.Instance.ShowPolicyMenu();
}
```

### 关闭道具菜单
```csharp
UIManager.Instance.HidePolicyMenu();
```

---

## 六、测试建议

1. **测试跳过道具**：
   - 添加一个跳过道具到 inventory
   - 点击"道具"按钮打开菜单
   - 点击跳过道具，应该直接跳到下一个事件

2. **测试调控道具**：
   - 添加一个调控道具（设置数值变化）
   - 点击使用后，观察五大数值变化
   - 检查是否触发结局判定

3. **测试免死道具**：
   - 添加一个免死道具（设置对应的 deathImmunity）
   - 选择一个会导致死亡的选项
   - 应该弹出免死确认窗口
   - 点击"使用"后数值恢复到选项点击前

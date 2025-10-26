# StatEffectController 使用说明

## 功能介绍
`StatEffectController` 是一个统计数值变化特效控制器，当游戏中的数值（国君、贵族、士族、外臣、国人）发生变化时：
- **数值增加**：显示增加特效图片
- **数值下降**：显示下降特效图片
- **数值变化会实时反映在填充图片（Filled Image）上**

## 使用步骤

### 1. 在 Unity 场景中设置

1. 为每个需要显示特效的数值创建一个 GameObject
2. 为该 GameObject 添加 `StatEffectController` 组件
3. 在 Inspector 面板中配置：

#### 基本设置
- **Stat Type**: 选择数值类型
  - King（国君）
  - Noble（贵族）
  - Scholar（士族）
  - Foreign（外臣）
  - People（国人）

#### 特效图片设置
- **Increase Effect Image**: 拖入数值增加时显示的特效图片（Image 组件）
- **Decrease Effect Image**: 拖入数值下降时显示的特效图片（Image 组件）
- **Value Filled Image**: 拖入显示当前数值百分比的填充图片（Image 组件，类型应设置为 Filled）

#### 特效参数设置
- **Effect Duration**: 特效显示时长（秒），默认 1 秒
- **Effect Fade Curve**: 特效淡入淡出曲线，控制透明度变化

### 2. 准备特效图片

#### 增加特效图片（Increase Effect Image）
- 可以是向上的箭头、闪光效果、绿色光晕等
- 建议使用半透明效果
- 图片类型可以是 Simple 或 Filled

#### 下降特效图片（Decrease Effect Image）
- 可以是向下的箭头、暗淡效果、红色光晕等
- 建议使用半透明效果
- 图片类型可以是 Simple 或 Filled

#### 数值填充图片（Value Filled Image）
- **必须设置为 Filled 类型**
- Fill Method 可以选择：
  - Horizontal（水平填充）
  - Vertical（垂直填充）
  - Radial 360（环形填充）
- Fill Amount 会根据数值自动更新（当前值 / 最大值）

### 3. 场景层级结构示例

```
StatDisplay
├── KingStatController (StatEffectController)
│   ├── IncreaseEffect (Image) - 增加特效
│   ├── DecreaseEffect (Image) - 下降特效
│   └── ValueBar (Image, Type: Filled) - 数值条
├── NobleStatController (StatEffectController)
│   ├── IncreaseEffect (Image)
│   ├── DecreaseEffect (Image)
│   └── ValueBar (Image, Type: Filled)
└── ... (其他数值的控制器)
```

### 4. 工作原理

1. **自动初始化**: 脚本启动时会自动获取 GameControl.Instance.stats
2. **事件监听**: 订阅对应数值类型的变化事件
3. **变化检测**: 当数值变化时，比较新旧值
   - 新值 > 旧值：显示增加特效
   - 新值 < 旧值：显示下降特效
4. **特效播放**: 使用协程播放淡入淡出动画
5. **填充更新**: 同时更新数值填充图片的 fillAmount

## 注意事项

1. **StatModel 修改**：为了支持事件触发，`StatModel.cs` 已经修改，将数值字段改为属性
2. **事件触发**：所有通过属性赋值的操作都会自动触发事件
3. **性能优化**：特效图片在不显示时会自动隐藏，避免性能浪费
4. **多个控制器**：每个数值可以有独立的特效控制器，互不干扰

## 自定义特效

### 修改特效时长
在 Inspector 中调整 `Effect Duration` 参数

### 自定义淡入淡出曲线
点击 `Effect Fade Curve` 在动画曲线编辑器中自定义：
- 默认曲线：从 1（完全显示）到 0（完全透明）
- 可以创建复杂的闪烁效果、脉冲效果等

### 扩展功能
如需添加更多效果（如缩放、旋转、位置变化），可以修改 `PlayEffectCoroutine` 方法

## 测试
1. 运行游戏
2. 触发数值变化（如选择影响数值的选项）
3. 观察特效是否正确显示
4. 检查填充图片是否正确更新

## 故障排除

### 特效不显示
- 检查 Image 组件是否正确拖入
- 确认图片材质和 Alpha 通道正常
- 检查 Canvas 层级是否正确

### 填充不更新
- 确认 Value Filled Image 的 Image Type 是否设置为 Filled
- 检查 Fill Method 设置是否正确

### 事件不触发
- 确认 GameControl.Instance 存在
- 确认 stats 引用正确
- 使用 Debug.Log 检查数值是否真的变化了

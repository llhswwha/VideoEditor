# 分辨率设置功能方案

目标
- 在“转码”界面增加分辨率设置项，支持三种模式：预设分辨率、用户自定义宽高、基于当前分辨率的百分比缩放。
- 最终结果用于构建 FFmpeg 的 scale 参数（例如 `-vf scale=WIDTH:HEIGHT`），并在需要时自动调整为偶数以兼容编码器。

功能要点
- 预设：常用分辨率（原始、3840x2160、1920x1080、1280x720、854x480、640x360、自定义）。
- 自定义：用户输入宽度和高度（正整数）。
- 百分比：基于当前视频分辨率按百分比缩放（例如 50%），自动计算新宽高。
- 偶数调整：多数编码器需要偶数宽高，提供 `EnforceEven` 选项（默认开启），对计算结果做四舍五入并调整为最近偶数。
- 验证与提示：输入校验、最小/最大限制、当 source 分辨率未知时禁用百分比选项并显示提示。

UI 设计（XAML）
- 在 `转码` 标签下新增一个 `GroupBox`：`分辨率（Resolution）`
- 控件：
  - `ComboBox` `cboResolutionPreset`：列出预设（含 `自定义` 和 `原始`）。
  - `TextBox` `txtCustomWidth`、`txtCustomHeight`：自定义宽高，仅在 `自定义` 模式可编辑。
  - `CheckBox` `chkUsePercentage`：启用百分比模式。
  - `Slider` / `TextBox` `sliderResolutionPercent` / `txtResolutionPercent`：选择百分比范围（例如 10%–200%）。
  - `TextBlock` `lblComputedResolution`：显示计算后的分辨率预览。
  - `CheckBox` `chkEnforceEven`：是否调整到偶数（默认勾选）。

数据模型 / ViewModel
- 新增 `TranscodeResolutionViewModel` 或在现有 `TranscodeParameters` / 转码相关 VM 中添加属性：
  - `Preset`（string/enum）
  - `CustomWidth`（int?）, `CustomHeight`（int?）
  - `UsePercentage`（bool）
  - `Percentage`（double）
  - `ComputedWidth`（int?）, `ComputedHeight`（int?）
  - `SourceWidth`（int）, `SourceHeight`（int） — 由当前播放或选中文件信息初始化
  - `EnforceEven`（bool）
  - `Compute()` 方法：根据模式计算 `ComputedWidth/ComputedHeight` 并做校验与偶数调整
  - `ApplyCommand` / `ComputeCommand`（ICommand）用于 UI 触发计算或应用设置

计算规则
- 预设：直接返回预设宽高。
- 自定义：使用用户输入的宽高（验证为正整数），若为空则认为不改变分辨率。
- 百分比：如果 `SourceWidth/SourceHeight` 可用，计算为 `round(SourceWidth * Percentage/100)`，同理高度；若不可用则禁用百分比控件并提示。
- 偶数调整：若 `EnforceEven` 为 true，则 `if (value % 2 == 1) value += 1;`。
- 保持纵横比（可选扩展）：若用户只输入宽或高，可按 source 比例计算另一维度；或支持 `-1`/`-2` 策略传递给 FFmpeg（当前实现优先使用明确计算值以保证可预测性）。

FFmpeg 集成
- 在生成转码命令时，从 `TranscodeParameters`（或相应 VM）读取 `OutputWidth`/`OutputHeight`（或 `ComputedWidth/ComputedHeight`）：
  - 若均有值且与原尺寸不同，则在参数中插入 `-vf scale={width}:{height}`。
  - 若用户选择 `原始` 或两个值都为空则不添加 scale。
- 注意：若需要保持纵横比，可使用 `-2` 或 `-1` 作为高度/宽度占位符，但当前建议直接计算确定值（并做偶数调整）。

如何获取 Source 分辨率
- 优先从当前播放的 `VideoPlayerViewModel.VideoWidth/VideoHeight` 或所选文件的 `VideoFile.Width/Height` 获取。
- 若都不可用，则在 UI 中显示 "当前分辨率未知，百分比模式不可用" 并禁用百分比控件。

输入校验与用户提示
- 宽高必须为 1 至 8192（或项目允许的上限）之间的整数。
- 百分比限制范围 10%–200%（建议），可根据需求放宽。
- 当计算导致奇数且 `EnforceEven` 开启时，显示提示（例如："为兼容编码器已调整到偶数: 501->502"）。

实现步骤（代码级）
1. 在 `Models/TranscodeParameters.cs` 增加 `OutputWidth`, `OutputHeight`, `EnforceEven` 属性（已完成）。
2. 新增 `ViewModels/TranscodeResolutionViewModel.cs`（或将逻辑集成到现有转码 VM）：实现属性、`Compute()`、INotifyPropertyChanged、命令。
3. 修改 `MainWindow.xaml`：在 `转码` 标签页插入 `GroupBox`，增加控件并命名（参考上方命名），将控件事件/命令绑定到新 ViewModel。
4. 在 `MainWindow.xaml.cs` 构造或初始化时，将 `TranscodeResolutionViewModel` 注入 DataContext 或视图数据上下文（例如把它作为 `MainWindow` 的属性并在 XAML 通过 `ElementName` 绑定）。
5. 在 `VideoProcessingService.BuildTranscodeArguments` 中读取 `parameters.OutputWidth` / `parameters.OutputHeight`，若两者有值且不等于原始尺寸则插入 `-vf scale=...` 参数（注意位置：在 `-c:v` 前或后根据需求调整，但保证滤镜链正确）。
6. 添加输入校验逻辑（UI 即时校验或 Compute() 内校验），并在异常或不合法输入时禁用“开始转码”按钮或弹出提示。
7. 手动/单元测试：预设、自定义、百分比三种模式；注意 source 未知时百分比禁用；测试奇数处理与 FFmpeg 命令生成。

示例：从百分比到 FFmpeg 参数
- 当前分辨率：1000x600
- 百分比：50%
- 计算：500x300（若 EnforceEven 自动调整为最近偶数）
- FFmpeg 调用片段：`-vf scale=500:300`

兼容性与注意事项
- 大多数编码器要求偶数，建议默认开启偶数对齐。
- 若用户需要保持纵横比，可在未来扩展 UI 提供“仅指定宽（高度自动）”的选项并支持 FFmpeg 的 `-2`/`-1` 策略。
- UI 国际化：所有新增控件字符串应使用现有资源文件或保持中文风格一致。

预估工时
- ViewModel + 计算逻辑：0.5 天
- XAML 布局与绑定：0.5 天
- 命令生成集成与测试：0.5 天
- 总计：约 1.5 天（视现有代码耦合与测试覆盖情况）

---

如果需要，我可以继续实现 ViewModel、XAML 插入与 VideoProcessingService 集成并提交代码修改。
using System;

namespace VideoEditor.Presentation.Models
{
    /// <summary>
    /// 转码模式枚举
    /// </summary>
    public enum TranscodeMode
    {
        /// <summary>
        /// 快速模式：使用 -c copy，速度快
        /// </summary>
        Fast,
        
        /// <summary>
        /// 标准模式：H.264 + AAC，兼容性好
        /// </summary>
        Standard,
        
        /// <summary>
        /// 高质量模式：低 CRF，文件较大
        /// </summary>
        HighQuality,
        
        /// <summary>
        /// 压缩模式：H.265，体积小
        /// </summary>
        Compress
    }

    /// <summary>
    /// 转码参数
    /// </summary>
    public class TranscodeParameters
    {
        /// <summary>
        /// 转码模式
        /// </summary>
        public TranscodeMode Mode { get; set; } = TranscodeMode.Standard;

        /// <summary>
        /// 输出格式（MP4、MKV、AVI等）
        /// </summary>
        public string OutputFormat { get; set; } = "MP4";

        /// <summary>
        /// 视频编码器（H.264、H.265等）
        /// </summary>
        public string VideoCodec { get; set; } = "H.264 / AVC";

        /// <summary>
        /// 音频编码器（AAC、MP3等）
        /// </summary>
        public string AudioCodec { get; set; } = "AAC (推荐)";

        /// <summary>
        /// CRF值（18-28，18=极高质量，23=推荐，28=较低质量）
        /// </summary>
        public int CRF { get; set; } = 23;

        /// <summary>
        /// 音频比特率（kbps）
        /// </summary>
        public string AudioBitrate { get; set; } = "256 kbps (推荐)";

        /// <summary>
        /// 是否启用双通道编码
        /// </summary>
        public bool DualPass { get; set; } = false;

        /// <summary>
        /// 是否启用硬件加速
        /// </summary>
        public bool HardwareAcceleration { get; set; } = false;

        /// <summary>
        /// 是否保留元数据
        /// </summary>
        public bool KeepMetadata { get; set; } = true;

        /// <summary>
        /// 输出宽度 (像素)，若为 null 表示不改变分辨率
        /// </summary>
        public int? OutputWidth { get; set; }

        /// <summary>
        /// 输出高度 (像素)，若为 null 表示不改变分辨率
        /// </summary>
        public int? OutputHeight { get; set; }

        /// <summary>
        /// 是否强制为偶数（多数编码器要求偶数宽高）
        /// </summary>
        public bool EnforceEven { get; set; } = true;

        /// <summary>
        /// 是否使用百分比缩放（基于源分辨率的百分比），若为 true 则 UI/逻辑应根据 Percentage 计算 OutputWidth/OutputHeight
        /// </summary>
        public bool UsePercentage { get; set; } = false;

        /// <summary>
        /// 百分比缩放值（默认 80 表示 80%）
        /// </summary>
        public double Percentage { get; set; } = 80.0;
    }
}


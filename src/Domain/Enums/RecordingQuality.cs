namespace WebRtcServer.Domain.Enums;

/// <summary>
/// Enumeração para definir a qualidade de gravação
/// </summary>
public enum RecordingQuality
{
    /// <summary>
    /// Qualidade baixa - menor tamanho de arquivo
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// Qualidade média - balanceamento entre qualidade e tamanho
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// Qualidade padrão
    /// </summary>
    Standard = 3,
    
    /// <summary>
    /// Qualidade alta - melhor qualidade, maior tamanho de arquivo
    /// </summary>
    High = 4,
    
    /// <summary>
    /// Qualidade ultra alta - máxima qualidade disponível
    /// </summary>
    UltraHigh = 5
}
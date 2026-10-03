namespace App.Shared.Common.Enums;

/// <summary>Decisión del filtro de tramas GPS repetidas.</summary>
public enum GpsGateDecision
{
    /// <summary>La trama es nueva o relevante: se persiste.</summary>
    Passed = 0,

    /// <summary>La trama repite un punto ya guardado: se descarta.</summary>
    Suppressed = 1,
}

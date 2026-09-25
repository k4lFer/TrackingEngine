namespace App.Shared.Common.Enums;

/// <summary>Plataforma/protocolo del dispositivo GPS (afecta qué decodificador aplica).</summary>
public enum DeviceKind
{
    Generic = 0,
    Traccar = 1,
    Tk103 = 2,
    Teltonika = 3,
    Queclink = 4,
    SinoTrack = 5,
}
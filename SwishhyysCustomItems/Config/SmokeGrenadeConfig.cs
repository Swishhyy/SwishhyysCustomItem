using System.ComponentModel;

public class SmokeGrenadeConfig
{
    [Description("Whether to remove the smoke effect after a delay")]
    public bool RemoveSmoke { get; set; } = true;

    [Description("How long the smoke cloud remains before being removed (seconds)")]
    public float SmokeTime { get; set; } = 10f;

    [Description("Scale factor for the smoke effect (0.01 is small, 1.0 is large)")]
    public float SmokeScale { get; set; } = 0.01f;

    [Description("Maximum diameter the smoke cloud can expand to")]
    public float SmokeDiameter { get; set; } = 0.0f;

    [Description("Show hint message to players caught in smoke")]
    public bool ShowSmokeMessage { get; set; } = false;

    [Description("Message to show to players caught in smoke")]
    public string SmokeMessage { get; set; } = "You're in a smoke cloud!";

    [Description("Duration to show smoke message (seconds)")]
    public float MessageDuration { get; set; } = 3f;

    [Description("Enable debug logging")]
    public bool EnableDebugLogging { get; set; } = false;
}
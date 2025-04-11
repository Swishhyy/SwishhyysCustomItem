using System.ComponentModel;

public class ImpactGrenadeConfig
{
    [Description("Maximum damage dealt at the center of explosion")]
    public float MaximumDamage { get; set; } = 115f;

    [Description("Minimum damage dealt at the edge of explosion radius")]
    public float MinimumDamage { get; set; } = 35f;

    [Description("Radius of the explosion damage")]
    public float DamageRadius { get; set; } = 7f;

    [Description("Message shown to players hit by the grenade")]
    public string EffectMessage { get; set; } = "You were hit by an impact grenade!";

    [Description("Duration to show the effect message in seconds")]
    public float MessageDuration { get; set; } = 3f;

    [Description("Whether to show hit message to affected players")]
    public bool ShowEffectMessage { get; set; } = true;

    [Description("Enable debug logging")]
    public bool EnableDebugLogging { get; set; } = false;
}
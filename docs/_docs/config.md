---
layout: default
title: Configuration
permalink: /docs/config/
---

# Configuration

Below is the complete configuration reference for the Swishhyys Custom Items (SCI) plugin:

```yaml
s_c_i:
  # Whether the plugin is enabled or not
  is_enabled: true
  # Whether to enable debug logging
  debug: false
  # Discord webhook URL for notifications
  discord_webhook: ''
  expired_s_c_p500:
    # Default duration for applied effects (in seconds)
    default_effect_duration: 10
    # Category probabilities (must sum to 100)
    category_chances:
      Positive: 50
      Negative: 30
      SCP: 20
    # Positive effects with their individual chances and intensities
    positive_effects:
      MovementBoost:
        # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 5
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      # Additional positive effects omitted for brevity
    
    # Negative effects with their individual chances and intensities
    negative_effects:
      Concussed:
        # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 8
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      # Additional negative effects omitted for brevity
      
    # SCP-like effects with their individual chances and intensities
    s_c_p_effects:
      Asphyxiated:
        # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 10
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      # Additional SCP effects omitted for brevity
      
    # Healing fallback settings if no effects are applied
    heal_fallback:
      # Minimum healing amount when no effects are applied
      min_heal: 15
      # Maximum healing amount when no effects are applied
      max_heal: 35
      
  adrenaline_s_c_p500:
    # Speed multiplier for the player when using the adrenaline pills
    speed_multiplier: 1.79999995
    # Duration of the adrenaline effect in seconds
    effect_duration: 25
    # Cooldown between using adrenaline pills in seconds
    cooldown: 5
    # Amount of stamina to restore when using the pills (0-100)
    stamina_restore_amount: 100
    # Duration of the hint message in seconds
    hint_duration: 5
    # Duration of the exhaustion effect after adrenaline wears off
    exhaustion_duration: 5
    # Message shown when adrenaline effect begins
    activation_message: '<color=yellow>You feel a rush of adrenaline!</color>'
    # Message shown when adrenaline effect ends
    exhaustion_message: '<color=red>You feel exhausted after the adrenaline rush...</color>'
    # Message shown when trying to use during cooldown
    cooldown_message: 'You must wait before using another pill!'
    
  suicide_s_c_p500:
    # Chance that the player survives the explosion (0-100)
    survival_chance: 5
    # Amount of health to give the player if they survive
    survival_health_amount: 5
    # Maximum explosion damage to the user
    user_damage: 1000
    # Maximum damage to nearby players
    max_nearby_player_damage: 70
    # Explosion radius (in meters)
    explosion_radius: 10
    # Duration of the hint message (in seconds)
    hint_duration: 5
    # Message shown to player when they survive
    survival_message: 'You consumed the suicide pills, but somehow survived the explosion!'
    # Message shown to player when they won't survive
    death_message: 'You consumed the suicide pills...'
    
  cluster_grenade:
    # Number of child grenades to spawn after the initial explosion
    child_grenade_count: 3
    # Fuse time for child grenades in seconds
    child_grenade_fuse_time: 1.5
    # Delay between spawning each child grenade in seconds
    child_grenade_delay: 0.100000001
    # Maximum radius for random spread of child grenades
    spread_radius: 3
    # Additional cluster grenade settings omitted for brevity
    
  impact_grenade:
    # Maximum damage dealt at the center of explosion
    maximum_damage: 115
    # Minimum damage dealt at the edge of explosion radius
    minimum_damage: 35
    # Radius of the explosion damage
    damage_radius: 7
    # Additional impact grenade settings omitted for brevity
    
  smoke_grenade:
    # Whether to remove the smoke effect after a delay
    remove_smoke: true
    # How long the smoke cloud remains before being removed (seconds)
    smoke_time: 10
    # Additional smoke grenade settings omitted for brevity
    
  railgun:
    # Unique ID for the railgun
    id: 107
    # Maximum damage dealt by the railgun
    damage: 75
    # Maximum range of the railgun beam in meters
    range: 50
    # Width of the beam/hit detection in meters
    beam_width: 0.5
    # Whether the railgun creates an explosion at the impact point
    spawn_explosive: true
    # Maximum number of railguns that can spawn in a round
    spawn_limit: 1
    # Cooldown between shots in seconds
    cooldown: 10
    # Enable debug logging
    enable_debug_logging: false
```

> Note: Some configurations are abbreviated in this display. For full details, check the plugin's config file.
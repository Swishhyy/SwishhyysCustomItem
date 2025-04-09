# Swishhyys Custom Items (SCI)

## What is this?

This is a plugin to add cool custom items to SCP: SL using the Exiled Framework. This is mainly produced out of a need to grant more items to the server I host, NNR. To allow my community to have a bit more fun with the game, however, it's also being used as a learning tool for me to make Exiled plugins. I'm hoping the work I'll put into this project will pay off, but we will never know!

### Permissions

```
sci.admin  // Admin perms within the plugin
```

### Commands

```
Grant Command: sci give {playerid} {itemid}
```

### Item List (with ids)

```
Adrenaline SCP 500 Pills (id = 101) // works similar to stims, but effect is more potent; however will damage the user
Expired SCP 500 Pills (id = 102) //grants random effect upon use
Suicide SCP 500 Pills (id = 103) // operates similar to pink candy with a 5% chance for you to survive
```

## I want to see what this plugin does before I add it

I host my server that will feature this plugin amongst other future projects I may work on to further enhance the game.

### Contributions

Check out my [License](https://github.com/Swishhyy/SCI/blob/main/LICENSE.txt), but primarily, I won't focus on suggestions, as this plugin is developed mainly for my server, NNR. Sometimes, I may take suggestions.
You are welcome to fork, but please properly credit me, and I will accept merges as well.

### How to read my updates and what you should do

1.0.0 [Sematic Versioning](https://semver.org/)

The first digit is the major version; if this changes, it means the previous config or code is hugely changed. In short, delete everything and reinstall
The second digit is a minor version. In most cases, your existing configuration will be fine; just update the DLL
The Third digit is a patch version, which means very minor code patches so not any significant changes. This could be fixing a bug or updating the version so it displays properly in the server console.

### WARNING:

This is my first Exiled plugin, so there may be issues I'm not entirely sure how to fix/handle as I'm also picking up more advanced C# as well. The best way to reach me regarding issues with this plugin or others will be on discord or the issue tracker.

## Config

```
s_c_i:
# Whether the plugin is enabled.
  is_enabled: true
  # Whether debug messages should be shown in the console.
  debug: false
  # Configuration for Expired SCP-500 Pills
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
      Scp207:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 3
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Invigorated:
      # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 5
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      BodyshotReduction:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 3
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Invisible:
      # Chance for this effect to be selected (within its category)
        chance: 5
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 2
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Vitality:
      # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 4
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      DamageReduction:
      # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 3
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      RainbowTaste:
      # Chance for this effect to be selected (within its category)
        chance: 5
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 1
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      AntiScp207:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 1
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
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
      Bleeding:
      # Chance for this effect to be selected (within its category)
        chance: 12
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 10
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Burned:
      # Chance for this effect to be selected (within its category)
        chance: 8
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 6
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Deafened:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 8
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Exhausted:
      # Chance for this effect to be selected (within its category)
        chance: 12
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 10
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Flashed:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 6
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Disabled:
      # Chance for this effect to be selected (within its category)
        chance: 5
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 6
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Ensnared:
      # Chance for this effect to be selected (within its category)
        chance: 8
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 8
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Hemorrhage:
      # Chance for this effect to be selected (within its category)
        chance: 5
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 10
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Poisoned:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 10
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
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
      CardiacArrest:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 10
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Decontaminating:
      # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 8
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      SeveredHands:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 1
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Stained:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 8
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      AmnesiaItems:
      # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 1
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      AmnesiaVision:
      # Chance for this effect to be selected (within its category)
        chance: 15
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 1
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
      Corroding:
      # Chance for this effect to be selected (within its category)
        chance: 10
        # Minimum intensity level for this effect
        min_intensity: 1
        # Maximum intensity level for this effect
        max_intensity: 8
        # Custom duration for this effect (in seconds). Set to 0 to use default duration.
        custom_duration: 0
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
```

using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace RasForSts2.Scripts.Patches;

/// <summary>
/// Bridges SetAnimationTrigger to AnimatedSprite2D for non-Spine characters (like Xila).
/// The Cast/Attack/Hit animations are defined directly in the .tscn SpriteFrames.
/// This patch simply plays the corresponding animation when the engine fires a trigger.
/// </summary>
public static class AnimatedSpriteAnimationBridge
{
    private static readonly Dictionary<NCreature, AnimatedSprite2D> _creatureSpriteCache = new();

    public static void InitializeCreatureVisuals(NCreatureVisuals visuals)
    {
        if (visuals == null || visuals.HasSpineAnimation) return;

        var sprite = FindAnimatedSprite(visuals);
        if (sprite == null) return;

        ConnectAnimationFinishedSignal(sprite);

        Log.Debug($"[AnimatedSpriteBridge] Initialized for {visuals.Name}");
    }

    private static AnimatedSprite2D FindAnimatedSprite(NCreatureVisuals visuals)
    {
        var body = visuals.GetNodeOrNull<Node2D>("%Visuals");
        if (body == null) return null;

        return body.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
    }

    private static void ConnectAnimationFinishedSignal(AnimatedSprite2D sprite)
    {
        sprite.AnimationFinished += () => HandleAnimationFinished(sprite);
    }

    private static void HandleAnimationFinished(AnimatedSprite2D sprite)
    {
        string currentAnim = sprite.Animation;
        if (currentAnim == "Cast" || currentAnim == "Attack" || currentAnim == "Hit")
        {
            sprite.Play("Idle");
        }
    }

    public static bool TryPlayAnimation(NCreature creature, string trigger)
    {
        if (creature?.Visuals == null || creature.Visuals.HasSpineAnimation) return false;

        if (!_creatureSpriteCache.TryGetValue(creature, out var sprite))
        {
            sprite = FindAnimatedSprite(creature.Visuals);
            if (sprite == null) return false;
            _creatureSpriteCache[creature] = sprite;
        }

        if (sprite.SpriteFrames?.HasAnimation(trigger) == true)
        {
            sprite.Play(trigger);
            return true;
        }

        return false;
    }

    public static void ClearCreatureCache(NCreature creature)
    {
        _creatureSpriteCache.Remove(creature);
    }
}

/// <summary>
/// Patches NCreatureVisuals._Ready() to connect the animation finished signal for non-Spine characters.
/// </summary>
[HarmonyPatch(typeof(NCreatureVisuals), "_Ready")]
public static class NCreatureVisualsReadyPatch
{
    public static void Postfix(NCreatureVisuals __instance)
    {
        AnimatedSpriteAnimationBridge.InitializeCreatureVisuals(__instance);
    }
}

/// <summary>
/// Patches NCreature.SetAnimationTrigger to bridge to AnimatedSprite2D for non-Spine characters.
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
public static class NCreatureSetAnimationTriggerPatch
{
    public static void Postfix(NCreature __instance, string trigger)
    {
        if (__instance.Visuals?.HasSpineAnimation == false)
        {
            AnimatedSpriteAnimationBridge.TryPlayAnimation(__instance, trigger);
        }
    }
}

/// <summary>
/// Patches NCreature._ExitTree to clear the sprite cache.
/// </summary>
[HarmonyPatch(typeof(NCreature), "_ExitTree")]
public static class NCreatureExitTreePatch
{
    public static void Postfix(NCreature __instance)
    {
        AnimatedSpriteAnimationBridge.ClearCreatureCache(__instance);
    }
}

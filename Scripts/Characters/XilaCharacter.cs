using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Utils;

namespace RasForSts2.Scripts.Characters;

[RegisterCharacter]
public class XilaCharacter : ModCharacterTemplate<XilaCardPool, XilaRelicPool, XilaPotionPool>
{
    public override Color NameColor => new(0.4f, 0.5f, 0.9f);
    public override Color EnergyLabelOutlineColor => new(0.4f, 0.5f, 0.9f);
    public override Color MapDrawingColor => new(0.4f, 0.5f, 0.9f);

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override int StartingHp => 75;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles
        .Silent()
        .WithScenes(new CharacterSceneAssetSet(
            VisualsPath: "res://RasForSts2/scenes/xila_character.tscn",
            EnergyCounterPath: "res://RasForSts2/scenes/xila_energy_counter.tscn",
            MerchantAnimPath: "res://RasForSts2/scenes/xila_character_merchant.tscn",
            RestSiteAnimPath: "res://RasForSts2/scenes/xila_character_rest_site.tscn"
        ))
        .WithUi(new CharacterUiAssetSet(
            IconTexturePath: "res://RasForSts2/images/visuals/xila_icon.png",
            IconPath: "res://RasForSts2/scenes/xila_icon.tscn",
            CharacterSelectBgPath: "res://RasForSts2/scenes/xila_bg.tscn",
            CharacterSelectIconPath: "res://RasForSts2/images/char_select_xila.png",
            CharacterSelectLockedIconPath: "res://RasForSts2/images/char_select_xila_locked.png"
        ));

    public override float AttackAnimDelay => 0.2f;
    public override float CastAnimDelay => 0f;

    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_slash",
        "vfx/vfx_heavy_slash",
        "vfx/vfx_attack_blunt",
        "vfx/vfx_bloody_impact"
    ];
}

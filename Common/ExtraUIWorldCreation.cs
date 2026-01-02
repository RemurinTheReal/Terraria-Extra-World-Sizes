using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using ExtraWorldSizes.Core;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExtraWorldSizes.Common;

public static class ExtraUIWorldCreation
{
    #region Properties
    private static GroupOptionButton<WorldSizeID>[] _sizeButtons = Array.Empty<GroupOptionButton<WorldSizeID>>();
    private static WorldSizeID _sizeChoice = WorldSizeID.Medium;
    #endregion
    
    #region Reflection Properties
    private static MethodInfo _assignRandomWorldNameMethod;
    private static MethodInfo _assignRandomWorldSeedMethod;
    private static MethodInfo _updateInputFieldsMethod;
    private static MethodInfo _updatePreviewPlateMethod;
    
    private static FieldInfo _descriptionTextField;
    private static FieldInfo _optionSizeField;
    #endregion
    
    public static void OnLoad()
    {
        On_UIWorldCreation.SetDefaultOptions += On_SetDefaultOptions;
        On_UIWorldCreation.AddWorldSizeOptions += On_AddWorldSizeOptions;
        IL_UIWorldCreation.FinishCreatingWorld += Il_FinishCreationWorld;
        On_UIWorldCreation.UpdateSliders += On_UIWorldCreationOnUpdateSliders;
    }

    public static void OnUnload()
    {
        On_UIWorldCreation.SetDefaultOptions -= On_SetDefaultOptions;
        On_UIWorldCreation.AddWorldSizeOptions -= On_AddWorldSizeOptions;
        IL_UIWorldCreation.FinishCreatingWorld -= Il_FinishCreationWorld;
        On_UIWorldCreation.UpdateSliders -= On_UIWorldCreationOnUpdateSliders;
    }

 

    #region Hooks
    private static void On_UIWorldCreationOnUpdateSliders(On_UIWorldCreation.orig_UpdateSliders orig, UIWorldCreation self)
    {
        foreach (var t in _sizeButtons)
        {
            t.SetCurrentOption(_sizeChoice);
        }
        
        var difficultyButtons = (Array)typeof(UIWorldCreation).GetField("_difficultyButtons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(self);
        if (difficultyButtons != null)
        {
            foreach (var button in difficultyButtons)
            {
                if (button == null) continue;

                var difficultyChoiceField = button.GetType().GetField("_optionDifficulty", BindingFlags.NonPublic | BindingFlags.Instance);
                if (difficultyChoiceField == null) continue;
                
                var methodInfo = button.GetType().GetMethod("SetCurrentOption", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo?.Invoke(button, new[] { Enum.ToObject(button.GetType().GenericTypeArguments[0], difficultyChoiceField.GetValue(button) ?? 0) });
            }
        }

        var evilButtons = (Array)typeof(UIWorldCreation).GetField("_evilButtons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(self);
        if (evilButtons == null) return;
        
        foreach (var button in evilButtons)
        {
            if (button == null) continue;
            
            var evilChoiceField = button.GetType().GetField("_optionEvil", BindingFlags.NonPublic | BindingFlags.Instance);
            if (evilChoiceField == null) continue;
                
            var methodInfo = button.GetType().GetMethod("SetCurrentOption", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo?.Invoke(button, new[] { Enum.ToObject(button.GetType().GenericTypeArguments[0], evilChoiceField.GetValue(button) ?? 0) });
        }
    }
    
    private static void On_SetDefaultOptions(On_UIWorldCreation.orig_SetDefaultOptions orig, UIWorldCreation self)
    {
        AssignRandomWorldName(self);
        AssignRandomWorldSeed(self);
        UpdateInputFields(self);
        
        var config = ModContent.GetInstance<ExtraConfig>();
        
        _sizeChoice = ModContent.GetInstance<ExtraConfig>().DefaultWorldSize;
        if (_sizeChoice == WorldSizeID.Huge && !config.EnableHugeWorldSize) _sizeChoice = WorldSizeID.Large;
        if (_sizeChoice == WorldSizeID.Tiny && !config.EnableTinyWorldSize) _sizeChoice = WorldSizeID.Small;

        foreach (var button in _sizeButtons) button.SetCurrentOption(_sizeChoice);

        var difficultyButtons = (Array)typeof(UIWorldCreation).GetField("_difficultyButtons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(self);
        if (difficultyButtons != null)
        {
            foreach (var button in difficultyButtons)
            {
                if (button == null) continue;
                
                var methodInfo = button.GetType().GetMethod("SetCurrentOption", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo?.Invoke(button, new[] { Enum.ToObject(button.GetType().GenericTypeArguments[0], 0) });
            }
        }

        var evilButtons = (Array)typeof(UIWorldCreation).GetField("_evilButtons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(self);
        if (evilButtons == null) return;
        
        foreach (var button in evilButtons)
        {
            if (button == null) continue;
                
            var methodInfo = button.GetType().GetMethod("SetCurrentOption", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo?.Invoke(button, new[] { Enum.ToObject(button.GetType().GenericTypeArguments[0], 0) });
        }
    }
    private static void On_AddWorldSizeOptions(On_UIWorldCreation.orig_AddWorldSizeOptions orig, UIWorldCreation self, UIElement container, float accumulatedHeight, UIElement.MouseEvent clickEvent, string tagGroup, float usableWidthPercent)
    {
        var worldSizeIDs = new WorldSizeID[] {
            WorldSizeID.Tiny,
            WorldSizeID.Small,
            WorldSizeID.Medium,
            WorldSizeID.Large,
            WorldSizeID.Huge
        };

        var mod = ModContent.GetInstance<ExtraMain>();
        if (mod == null) return;
        
        var config = ModContent.GetInstance<ExtraConfig>();
        
        var titles = new LocalizedText[] {
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeTiny"), () => "Tiny"),
            Lang.menu[92],
            config.EnableHugeWorldSize && config.EnableTinyWorldSize ? Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeHMedium"), () => "Med.") : Lang.menu[93],
            Lang.menu[94],
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeHuge"), () => "Huge")
        };
        
        var descriptions = new LocalizedText[] {
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldDescriptionSizeTiny"), () => "Quick and compact for unique challenges and testing."),
            Language.GetText("UI.WorldDescriptionSizeSmall"),
            Language.GetText("UI.WorldDescriptionSizeMedium"),
            Language.GetText("UI.WorldDescriptionSizeLarge"),
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldDescriptionSizeHuge"), () => "Oversized for near endless exploration or large servers.")
        };

        var colors = new Color[] {
            Color.SkyBlue,
            Color.Cyan,
            Color.Lerp(Color.Cyan, Color.LimeGreen, 0.5f),
            Color.LimeGreen,
            Color.Green
        };

        var icons = new string[] {
            "Images/UI/WorldCreation/IconSizeSmall",
            "Images/UI/WorldCreation/IconSizeSmall",
            "Images/UI/WorldCreation/IconSizeMedium",
            "Images/UI/WorldCreation/IconSizeLarge",
            "Images/UI/WorldCreation/IconSizeLarge"
        };
        

        var enabledSizes = new List<WorldSizeID>();
        for (var i = 0; i < worldSizeIDs.Length; i++)
        {
            switch (i)
            {
                case (int)WorldSizeID.Tiny when !config.EnableTinyWorldSize:
                case (int)WorldSizeID.Huge when !config.EnableHugeWorldSize: 
                    continue;
                default:
                    enabledSizes.Add(worldSizeIDs[i]); break;
            }
        } 
        
        _sizeButtons = new GroupOptionButton<WorldSizeID>[enabledSizes.Count];
        
        for (var i = 0; i < _sizeButtons.Length; i++)
        {
            var sizeID = (int)enabledSizes[i];
            var groupOptionButton = new GroupOptionButton<WorldSizeID>((WorldSizeID)sizeID, titles[sizeID], descriptions[sizeID], colors[sizeID], icons[sizeID], 1f, 1f, 16f)
            {
                Width = StyleDimension.FromPixelsAndPercent(-1 * (_sizeButtons.Length - 1), 1f / _sizeButtons.Length * usableWidthPercent),
                Left = StyleDimension.FromPercent(1f - usableWidthPercent),
                HAlign = (float)i / (_sizeButtons.Length - 1)
            };
            
            groupOptionButton.Top.Set(accumulatedHeight, 0f);
            groupOptionButton.SetSnapPoint(tagGroup, i);
            
            groupOptionButton.OnLeftMouseDown += (_, element) => ClickSizeOption(self, element);
            groupOptionButton.OnMouseOver += (_, element) => ShowOptionDescription(self, element);
            groupOptionButton.OnMouseOut += (_, _) => ClearOptionDescription(self);
            
            container.Append(groupOptionButton);
            
            _sizeButtons[i] = groupOptionButton;
        }
    }
    private static void Il_FinishCreationWorld(ILContext il)
    {
        var mod = ModContent.GetInstance<ExtraMain>();
        if (mod == null) return;
        
        var ilCursor = new ILCursor(il);
        if (!ilCursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(WorldGen), nameof(WorldGen.setWorldSize))))
        {
            mod.Logger.Error($"Failed to find Instruction for {nameof(Il_FinishCreationWorld)}");
            return;
        }
        
        ilCursor.EmitDelegate(() =>
        {
            switch (_sizeChoice)
            {
                case WorldSizeID.Tiny:
                    Main.maxTilesX = ExtraWorldGen.WorldSizeTinyX;
                    Main.maxTilesY = ExtraWorldGen.WorldSizeTinyY;
                    WorldGen.setWorldSize();
                    break;
                case WorldSizeID.Huge:
                    Main.maxTilesX = ExtraWorldGen.WorldSizeHugeX;
                    Main.maxTilesY = ExtraWorldGen.WorldSizeHugeY;
                    WorldGen.setWorldSize();
                    break;
                case WorldSizeID.Small:
                case WorldSizeID.Medium:
                case WorldSizeID.Large: 
                default: break;
            }
        });
    }
    #endregion

    #region Helpers
    private static void AssignRandomWorldName(UIWorldCreation self)
    {
        _assignRandomWorldNameMethod ??= typeof(UIWorldCreation).GetMethod("AssignRandomWorldName", BindingFlags.Instance | BindingFlags.NonPublic);
        _assignRandomWorldNameMethod?.Invoke(self, Array.Empty<object>());
    }
    
    private static void AssignRandomWorldSeed(UIWorldCreation self)
    {
        _assignRandomWorldSeedMethod ??= typeof(UIWorldCreation).GetMethod("AssignRandomWorldSeed", BindingFlags.Instance | BindingFlags.NonPublic);
        _assignRandomWorldSeedMethod?.Invoke(self, Array.Empty<object>());
    }
    
    private static void UpdateInputFields(UIWorldCreation self)
    {
        _updateInputFieldsMethod ??= typeof(UIWorldCreation).GetMethod("UpdateInputFields", BindingFlags.Instance | BindingFlags.NonPublic);
        _updateInputFieldsMethod?.Invoke(self, Array.Empty<object>());
    }
    
    private static void ClearOptionDescription(UIWorldCreation self)
    {
        _descriptionTextField ??= typeof(UIWorldCreation).GetField("_descriptionText", BindingFlags.Instance | BindingFlags.NonPublic);
        (_descriptionTextField?.GetValue(self) as UIText)?.SetText(Language.GetText("UI.WorldDescriptionDefault"));
    }
    
    private static void UpdatePreviewPlate(UIWorldCreation self)
    {
        _updatePreviewPlateMethod ??= typeof(UIWorldCreation).GetMethod("UpdatePreviewPlate", BindingFlags.Instance | BindingFlags.NonPublic);
        _updatePreviewPlateMethod?.Invoke(self, Array.Empty<object>());
    }	
    
    private static void ClickSizeOption(UIWorldCreation self, UIElement listeningElement)
    {
        var groupOptionButton = listeningElement as GroupOptionButton<WorldSizeID>;
        if (groupOptionButton == null) return;
        
        _optionSizeField ??= typeof(UIWorldCreation).GetField("_optionSize", BindingFlags.Instance | BindingFlags.NonPublic);
        _optionSizeField?.SetValue(self, Math.Clamp((int)groupOptionButton.OptionValue - 1, 0, 2));
        
        _sizeChoice = groupOptionButton.OptionValue;
        
        foreach (var button in _sizeButtons)
            button.SetCurrentOption(groupOptionButton.OptionValue);

        UpdatePreviewPlate(self);
    }
    
    private static void ShowOptionDescription(UIWorldCreation self, UIElement listeningElement)
    {
        var localizedText = (listeningElement as GroupOptionButton<WorldSizeID>)?.Description;
        if (localizedText == null) return;
        
        _descriptionTextField ??= typeof(UIWorldCreation).GetField("_descriptionText", BindingFlags.Instance | BindingFlags.NonPublic);
        (_descriptionTextField?.GetValue(self) as UIText)?.SetText(localizedText);
    }
    #endregion
}
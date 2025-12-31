using System;
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
using Terraria.WorldBuilding;

namespace ExtraWorldSizes.Common;

public static class ExtraUIWorldCreation
{
    private static GroupOptionButton<WorldSizeID>[] _sizeButtons = Array.Empty<GroupOptionButton<WorldSizeID>>();
    private static WorldSizeID _sizeChoice;
    private static MethodInfo assignRandomWorldNameMethod;
    private static MethodInfo assignRandomWorldSeedMethod;
    private static MethodInfo updateInputFieldsMethod;
    private static MethodInfo updatePreviewPlateMethod;

    private static FieldInfo descriptionTextField;

    public static void OnLoad()
    {
        On_UIWorldCreation.SetDefaultOptions += OnSetDefaultOptions;
        On_UIWorldCreation.AddWorldSizeOptions += On_AddWorldSizeOptions;
        IL_UIWorldCreation.FinishCreatingWorld += IlFinishCreationWorld;

    }

    public static void OnUnload()
    {
        On_UIWorldCreation.SetDefaultOptions -= OnSetDefaultOptions;
        On_UIWorldCreation.AddWorldSizeOptions -= On_AddWorldSizeOptions;
        IL_UIWorldCreation.FinishCreatingWorld -= IlFinishCreationWorld;
    }
    
    private static void IlFinishCreationWorld(ILContext il)
    {
        var mod = ModContent.GetInstance<ExtraMain>();
        
        var ilCursor = new ILCursor(il);
        if (ilCursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(WorldGen), nameof(WorldGen.setWorldSize))))
        {
            ilCursor.EmitDelegate(() =>
            {
                mod.Logger.Warn("Found");
                switch (_sizeChoice)
                {
                    case WorldSizeID.Tiny:
                        Main.maxTilesX = 2100;
                        Main.maxTilesY = 600;
                        break;
                    case WorldSizeID.Small:
                        Main.maxTilesX = 4200;
                        Main.maxTilesY = 1200;
                        break;
                    case WorldSizeID.Medium:
                        Main.maxTilesX = 6400;
                        Main.maxTilesY = 1800;
                        break;
                    case WorldSizeID.Large:
                        Main.maxTilesX = 8400;
                        Main.maxTilesY = 2400;
                        break;
                    case WorldSizeID.Huge:
                        Main.maxTilesX = 10500;
                        Main.maxTilesY = 3000;
                        break;
                }
                
                WorldGen.setWorldSize();
            });
        }
        else
        {
            throw new Exception();
        }
    }
    
    private static void AssignRandomWorldName(UIWorldCreation self)
    {
        assignRandomWorldNameMethod ??= typeof(UIWorldCreation).GetMethod("AssignRandomWorldName", BindingFlags.Instance | BindingFlags.NonPublic);
        assignRandomWorldNameMethod?.Invoke(self, Array.Empty<object>());
    }
    
    private static void AssignRandomWorldSeed(UIWorldCreation self)
    {
        assignRandomWorldSeedMethod ??= typeof(UIWorldCreation).GetMethod("AssignRandomWorldSeed", BindingFlags.Instance | BindingFlags.NonPublic);
        assignRandomWorldSeedMethod?.Invoke(self, Array.Empty<object>());
    }
    
    private static void UpdateInputFields(UIWorldCreation self)
    {
        updateInputFieldsMethod ??= typeof(UIWorldCreation).GetMethod("UpdateInputFields", BindingFlags.Instance | BindingFlags.NonPublic);
        updateInputFieldsMethod?.Invoke(self, Array.Empty<object>());
    }


    private static void OnSetDefaultOptions(On_UIWorldCreation.orig_SetDefaultOptions orig, UIWorldCreation self)
    {
        AssignRandomWorldName(self);
        AssignRandomWorldSeed(self);
        UpdateInputFields(self);

        _sizeChoice = ModContent.GetInstance<ExtraConfig>().DefaultWorldSize;
        foreach (var button in _sizeButtons) button.SetCurrentOption(_sizeChoice);

        var difficultyButtons = (Array)typeof(UIWorldCreation)
            .GetField("_difficultyButtons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(self);
        if (difficultyButtons != null)
        {
            foreach (var button in difficultyButtons)
            {
                if (button == null) continue;

                Type enumType = button.GetType().GenericTypeArguments[0];
                int rawValue = 0;

                object boxedEnum = Enum.ToObject(enumType, rawValue);

                MethodInfo m = button.GetType().GetMethod("SetCurrentOption",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                m.Invoke(button, new[] { boxedEnum });
            }
        }

        var evilButtons = (Array)typeof(UIWorldCreation)
            .GetField("_evilButtons", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(self);
        if (evilButtons != null)
        {
            foreach (var button in evilButtons)
            {
                if (button == null) continue;

                Type enumType = button.GetType().GenericTypeArguments[0];
                int rawValue = 0;

                object boxedEnum = Enum.ToObject(enumType, rawValue);

                MethodInfo m = button.GetType().GetMethod("SetCurrentOption",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                m.Invoke(button, new[] { boxedEnum });
            }
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
        
        var titles = new LocalizedText[] {
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeTiny"), () => "Tiny"),
            Lang.menu[92],
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeHMedium"), () => "Med."),
            Lang.menu[94],
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeHuge"), () => "Huge")
        };
        
        var descriptions = new LocalizedText[] {
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldDescriptionSizeTiny"), () => "Compact for challenges and quick testing."),
            Language.GetText("UI.WorldDescriptionSizeSmall"),
            Language.GetText("UI.WorldDescriptionSizeMedium"),
            Language.GetText("UI.WorldDescriptionSizeLarge"),
            Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldDescriptionSizeHuge"), () => "Oversized for endless discovery or large servers.")
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
        
        var config = ModContent.GetInstance<ExtraConfig>();

        _sizeButtons = new GroupOptionButton<WorldSizeID>[worldSizeIDs.Length];
        for (var i = 0; i < _sizeButtons.Length; i++) {
            switch (i)
            {
                case (int)WorldSizeID.Tiny when !config.EnableTinyWorldSize:
                case (int)WorldSizeID.Huge when !config.EnableHugeWorldSize:
                    continue;
            }

            var groupOptionButton = new GroupOptionButton<WorldSizeID>(worldSizeIDs[i], titles[i], descriptions[i], colors[i], icons[i], 1f, 1f, 16f)
            {
                Width = StyleDimension.FromPixelsAndPercent(-1 * (_sizeButtons.Length - 1), 1f / _sizeButtons.Length * usableWidthPercent),
                Left = StyleDimension.FromPercent(1f - usableWidthPercent),
                HAlign = (float)i / (_sizeButtons.Length - 1)
            };
            
            groupOptionButton.Top.Set(accumulatedHeight, 0f);
            groupOptionButton.SetSnapPoint(tagGroup, i);
            
            groupOptionButton.OnLeftMouseDown += (evt, element) =>
            {
                ClickSizeOption(self, evt, element);
            };
            
            groupOptionButton.OnMouseOver += (evt, element) =>
            {
                ShowOptionDescription(self, evt, element);
            };

            groupOptionButton.OnMouseOut += (evt, element) =>
            {
                ClearOptionDescription(self, evt, element);
            };
            
            container.Append(groupOptionButton);
            
            _sizeButtons[i] = groupOptionButton;
        }
    }
    
    private static void ClickSizeOption(UIWorldCreation self, UIMouseEvent evt, UIElement listeningElement)
    {
        var groupOptionButton = (GroupOptionButton<WorldSizeID>)listeningElement;
        
        _sizeChoice = groupOptionButton.OptionValue;
        foreach (var button in _sizeButtons) button.SetCurrentOption(groupOptionButton.OptionValue);

        UpdatePreviewPlate(self);
    }
    
    private static void ClearOptionDescription(UIWorldCreation self, UIMouseEvent evt, UIElement listeningElement)
    {
        descriptionTextField ??= typeof(UIWorldCreation).GetField("_descriptionText", BindingFlags.Instance | BindingFlags.NonPublic);
        (descriptionTextField?.GetValue(self) as UIText)?.SetText(Language.GetText("UI.WorldDescriptionDefault"));
    }
    
    private static void UpdatePreviewPlate(UIWorldCreation self)
    {
        updatePreviewPlateMethod ??= typeof(UIWorldCreation).GetMethod("UpdatePreviewPlate", BindingFlags.Instance | BindingFlags.NonPublic);
        updatePreviewPlateMethod?.Invoke(self, Array.Empty<object>());
    }	

    private static void ShowOptionDescription(UIWorldCreation self, UIMouseEvent evt, UIElement listeningElement)
    {
        var localizedText = listeningElement switch
        {
            GroupOptionButton<WorldSizeID> button => button.Description,
            //GroupOptionButton<WorldDifficultyID> button => button.Description,
            //GroupOptionButton<WorldEvilID> button => button.Description,
            UICharacterNameButton button => button.Description,
            GroupOptionButton<bool> button => button.Description,
            _ => null
        };

        if (localizedText != null)
        {
            descriptionTextField ??= typeof(UIWorldCreation).GetField("_descriptionText", BindingFlags.Instance | BindingFlags.NonPublic);
            (descriptionTextField?.GetValue(self) as UIText)?.SetText(localizedText);
        }
    }
}
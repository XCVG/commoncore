using CommonCore.World;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CommonCore
{

    /// <summary>
    /// CommonCore game and game module parameters; specific to the game type
    /// </summary>
    public static partial class GameParams
    {

        public static bool ForceCampaignVarNumericEvaluation { get; private set; } = false; //if set, Conditionals will evaluate campaign vars as numeric, like Katana
        public static bool UseCampaignVarSimulatedLooseTyping { get; private set; } = true; //if set, Conditionals will auto-promote numeric types in campaign vars when setting

        public static bool UseDialoguePositionInheritance { get; private set; } = true; //Katana does not inherit the position property from the dialogue scene
        public static bool DialogueAlwaysExecuteFrameMicroscript { get; private set; } = true; //if set, will always execute the top-level microscript of a frame even on choice frames
        public static bool DialogueVerboseLogging { get; private set; } = false;

        public static bool UseCustomLeveling { get; private set; } = true;
        public static bool UseDerivedSkills { get; private set; } = true;

        public static PlayerViewType DefaultPlayerView { get; private set; } = PlayerViewType.PreferFirst;

        public static bool UseFallDamage { get; private set; } = true;
        public static bool UseRandomDamage { get; private set; } = true;
        public static float DamageFlashThreshold { get; private set; } = 0.01f; //very small because health is reported as a fraction

        public static bool AutoQuestNotifications { get; private set; } = true;

        public static bool AllowItemDropOutsideWorldScene { get; private set; } = false;

        public static bool ShowImpossibleSkillChecks { get; private set; } = true;
        public static bool AttemptImpossibleSkillChecks { get; private set; } = true;

        public static float DifficultyFactorMin { get; private set; } = 0.5f;
        public static float DifficultyFactorMax { get; private set; } = 2f;

        //RPG stats and skills with these names will be hidden from display
        public static IReadOnlyCollection<string> HideStats { get; private set; } = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        public static IReadOnlyCollection<string> HideSkills { get; private set; } = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase, "Magic", "MagicForce", "MagicElemental", "MagicDark" );

    }
}
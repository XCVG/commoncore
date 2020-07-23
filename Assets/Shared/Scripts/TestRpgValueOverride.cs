using CommonCore.RpgGame.Rpg;
using CommonCore.Scripting;
using System;

public static class TestRpgValueOverride
{ 
    
    [CCScript, CCScriptHook(Hook = ScriptHook.AfterModulesLoaded)]
    private static void TryRpgValuesOverride()
    {
        //disabled for now; awkward but it does work
        //RpgValues.SetOverride<Func<CharacterModel, float>>(nameof(RpgValues.MaxHealth), MaxHealth);
    }

    private static float MaxHealth(CharacterModel characterModel)
    {
        return 100000;
    }

}

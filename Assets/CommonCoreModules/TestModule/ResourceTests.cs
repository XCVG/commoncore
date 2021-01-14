using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;
using System.Linq;
using CommonCore.ResourceManagement;
using CommonCore.Config;

namespace CommonCore.TestModule
{

    /// <summary>
    /// Tests for new ResourceManager and resource subsystem
    /// </summary>
    public static class ResourceTests
    {
        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.AfterModulesLoaded)]
        private static void TriggerTest()
        {
            if (ConfigState.Instance.HasCustomFlag("RunTests"))
                TestResourceManagement();
        }

        [Command(alias = "Run", className = "ResourceManagementTest", useClassName = true)]
        private static void TestResourceManagement()
        {
            //WIP test resource manager

            //some crude unit-ish testing

            var rm = CCBase.ResourceManager;

            //test default
            {
                string rstring = rm.GetResource<TextAsset>("Test/ResourceTest", false).text;
                if (rstring == "NORMAL")
                    Debug.Log($"[ResourceTests] GetResource \"Test/ResourceTest\" ok (expected \"NORMAL\", got \"{rstring})\"");
                else
                    Debug.LogError($"[ResourceTests] GetResource \"Test/ResourceTest\" failed (expected \"NORMAL\", got \"{rstring}\")");
            }

            //test variants
            {
                string[] rstringvariants = rm.GetResourceAllVariants<TextAsset>("Test/ResourceTest", false)?.Select(r => r.text)?.ToArray() ?? new string[] { };
                if (rstringvariants.Length != 3)
                    Debug.LogWarning($"[ResourceTests] GetResourceAllVariants \"Test/ResourceTest\" got wrong number of resources (expected 3, got {rstringvariants.Length})");

                if (rstringvariants.SequenceEqual(new string[] { "CORE", "GAME", "NORMAL" }, StringComparer.Ordinal))
                    Debug.Log($"[ResourceTests] GetResourceAllVariants \"Test/ResourceTest\" ok\nExpected: [CORE, GAME, NORMAL], got {rstringvariants.ToNiceString()}");
                else
                    Debug.LogError($"[ResourceTests] GetResourceAllVariants \"Test/ResourceTest\" failed (mismatch)\nExpected: [CORE, GAME, NORMAL], got {rstringvariants.ToNiceString()}");
            }

            //test folder (GetResources)
            {
                string[] rstrings = rm.GetResources<TextAsset>("Test/", false)?.OrderBy(ta => ta.name)?.Select(r => r.text)?.ToArray() ?? new string[] { };
                string[] rstrings2 = rm.GetResources<TextAsset>("Test", false)?.OrderBy(ta => ta.name)?.Select(r => r.text)?.ToArray() ?? new string[] { };

                if (!rstrings.SequenceEqual(rstrings2))
                {
                    Debug.LogWarning($"[ResourceTests] Paths \"Test\" and \"Test/\" do not return the same result for GetResources!");
                }

                string[] expected = new string[] { "NORMAL", "NORMAL2"};
                if (rstrings.SequenceEqual(expected, StringComparer.Ordinal))
                    Debug.Log($"[ResourceTests] GetResources \"Test/\" ok\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");
                else
                    Debug.LogError($"[ResourceTests] GetResources \"Test/\" failed (mismatch)\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");
                
            }

            //test folder variants (GetResourcesAllVariants)
            {
                TextAsset[][] tassetlists = rm.GetResourcesAllVariants<TextAsset>("Test/", true);

                foreach(TextAsset[] talist in tassetlists)
                {
                    if (talist.Length == 0)
                        continue;

                    string[] expected;
                    if (talist[0].name == "ResourceTest") //a fragile test because name is not strongly guaranteed
                        expected = new string[] { "CORE", "GAME", "NORMAL" };
                    else if (talist[0].name == "ResourceTest2")
                        expected = new string[] { "CORE2", "GAME2", "NORMAL2" };
                    else
                        continue;

                    var rstrings = talist.Select(ta => ta.text);

                    if (rstrings.SequenceEqual(expected, StringComparer.Ordinal))
                        Debug.Log($"[ResourceTests] GetResourcesAllVariants \"Test/{talist[0].name}\" ok\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");
                    else
                        Debug.LogError($"[ResourceTests] GetResourcesAllVariants \"Test/{talist[0].name}\" failed (mismatch)\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");
                }
            }

            //test runtime injection
            {
                var rtasset = new TextAsset("RUNTIME");
                rtasset.name = "ResourceTest";

                rm.AddResource("Test/ResourceTest", rtasset, ResourcePriority.Explicit);

                string rstring = rm.GetResource<TextAsset>("Test/ResourceTest", false).text;
                if (rstring == "RUNTIME")
                    Debug.Log($"[ResourceTests] GetResource \"Test/ResourceTest\" after runtime inject ok (expected \"RUNTIME\", got \"{rstring}\")");
                else
                    Debug.LogError($"[ResourceTests] GetResource \"Test/ResourceTest\" after runtime inject failed (expected \"RUNTIME\", got \"{rstring}\")");
            }

            //test GetResourceAllVariants after runtime injection
            {
                string[] rstringvariants = rm.GetResourceAllVariants<TextAsset>("Test/ResourceTest", false)?.Select(r => r.text)?.ToArray() ?? new string[] { };
                if (rstringvariants.Length != 4)
                    Debug.LogWarning($"[ResourceTests] GetResourceAllVariants \"Test/ResourceTest\" after runtime inject got wrong number of resources (expected 4, got {rstringvariants.Length})");

                if (rstringvariants.SequenceEqual(new string[] { "CORE", "GAME", "NORMAL", "RUNTIME" }, StringComparer.Ordinal))
                    Debug.Log($"[ResourceTests] GetResourceAllVariants \"Test/ResourceTest\" after runtime inject ok\nExpected: [CORE, GAME, NORMAL, RUNTIME], got {rstringvariants.ToNiceString()}");
                else
                    Debug.LogError($"[ResourceTests] GetResourceAllVariants \"Test/ResourceTest\" after runtime inject failed (mismatch)\nExpected: [CORE, GAME, NORMAL, RUNTIME], got {rstringvariants.ToNiceString()}");
            }

            //test GetResources after runtime injection
            {
                string[] rstrings = rm.GetResources<TextAsset>("Test/", false)?.OrderBy(ta => ta.name)?.Select(r => r.text)?.ToArray() ?? new string[] { };

                string[] expected = new string[] { "RUNTIME", "NORMAL2" };
                if (rstrings.SequenceEqual(expected, StringComparer.Ordinal))
                    Debug.Log($"[ResourceTests] GetResources \"Test/\" ok\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");
                else
                    Debug.LogError($"[ResourceTests] GetResources \"Test/\" failed (mismatch)\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");

            }

            //test GetResourcesAllVariants after runtime injection
            {
                TextAsset[][] tassetlists = rm.GetResourcesAllVariants<TextAsset>("Test/", true);

                foreach (TextAsset[] talist in tassetlists)
                {
                    if (talist.Length == 0)
                        continue;

                    string[] expected;
                    if (talist[0].name == "ResourceTest") //a fragile test because name is not strongly guaranteed
                        expected = new string[] { "CORE", "GAME", "NORMAL", "RUNTIME" };
                    else if (talist[0].name == "ResourceTest2")
                        expected = new string[] { "CORE2", "GAME2", "NORMAL2" };
                    else
                        continue;

                    var rstrings = talist.Select(ta => ta.text);

                    if (rstrings.SequenceEqual(expected, StringComparer.Ordinal))
                        Debug.Log($"[ResourceTests] GetResourcesAllVariants \"Test/{talist[0].name}\" ok\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");
                    else
                        Debug.LogError($"[ResourceTests] GetResourcesAllVariants \"Test/{talist[0].name}\" failed (mismatch)\nExpected: {expected.ToNiceString()}, got {rstrings.ToNiceString()}");
                }
            }

            //test GetResource with non-matching (but castable) type
            {
                Texture tex = rm.GetResource<Texture>("Modules/TestModule/TypeTest/texture", false);

                if (tex != null)
                    Debug.Log($"[ResourceTests] GetResource \"Modules/TestModule/TypeTest/texture\" ok (asked for Texture, got {tex.GetType().Name})");
                else
                    Debug.LogError($"[ResourceTests] GetResource \"Modules/TestModule/TypeTest/texture\" failed (got nothing)");
            }

            //test GetResource with TypeExact = true (should get nothing)
            {
                Texture tex = rm.GetResource<Texture>("Modules/TestModule/TypeTest/texture", true);

                if (tex == null)
                    Debug.Log($"[ResourceTests] GetResource exactType \"Modules/TestModule/TypeTest/texture\" ok (asked for Texture, got nothing)");
                else
                    Debug.LogError($"[ResourceTests] GetResource exactType \"Modules/TestModule/TypeTest/texture\" failed (got a {tex.GetType().Name})");
            }

            //test GetResources with non-matching (but castable) type

            //inject Texture2D
            Texture2D injectedTex;
            {
                injectedTex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                rm.AddResource("Modules/TestModule/TypeTest/texture", injectedTex, ResourcePriority.Explicit);
            }

            //test GetResource after inject (if we ask for Texture, we should get our injected Texture2D)
            {
                Texture tex = rm.GetResource<Texture>("Modules/TestModule/TypeTest/texture", false);

                if (tex != null && tex == injectedTex)
                    Debug.Log($"[ResourceTests] GetResource \"Modules/TestModule/TypeTest/texture\" ok (asked for Texture, got {tex.GetType().Name})");
                else if (tex != null)
                    Debug.LogError($"[ResourceTests] GetResource \"Modules/TestModule/TypeTest/texture\" failed (got the wrong Texture2D)");
                else
                    Debug.LogError($"[ResourceTests] GetResource \"Modules/TestModule/TypeTest/texture\" failed (got nothing)");
            }

            //test GetResource type-exact after inject (if we ask for Texture, we should NOT get our injected Texture2D)
            {
                Texture tex = rm.GetResource<Texture>("Modules/TestModule/TypeTest/texture", true);

                if (tex == null)
                    Debug.Log($"[ResourceTests] GetResource exactType \"Modules/TestModule/TypeTest/texture\" ok (asked for Texture, got nothing)");
                else
                    Debug.LogError($"[ResourceTests] GetResource exactType \"Modules/TestModule/TypeTest/texture\" failed (got a {tex.GetType().Name})");
            }

            Texture2D injectedTex2D;
            RenderTexture injectedTexRender;

            //inject a Texture2D and a RenderTexture
            {
                injectedTex2D = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                rm.AddResource("Modules/TestModule/TypeTest/texture2", injectedTex2D, ResourcePriority.Explicit);

                injectedTexRender = new RenderTexture(64, 64, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                rm.AddResource("Modules/TestModule/TypeTest/texture2", injectedTexRender, ResourcePriority.Explicit);
            }

            //test GetResourceVariants after inject (non exact type)
            {
                Texture[] texArray = rm.GetResourceAllVariants<Texture>("Modules/TestModule/TypeTest/texture2", false);

                //we are expecting: any Texture, our Texture2D, our RenderTexture

                if (texArray.Contains(injectedTex2D) && texArray.Contains(injectedTexRender) && texArray.Length == 3)
                    Debug.Log($"[ResourceTests] GetResourceAllVariants after inject (non exact type) ok");
                else
                    Debug.LogError($"[ResourceTests] GetResourceAllVariants after inject (non exact type) failed");
            }

            //test GetResourceVariants after inject (exact type)
            {
                Texture2D[] texArray = rm.GetResourceAllVariants<Texture2D>("Modules/TestModule/TypeTest/texture2", true);

                if (texArray.Contains(injectedTex2D) && texArray.Length == 2)
                    Debug.Log($"[ResourceTests] GetResourceAllVariants after inject (exact type) ok");
                else
                    Debug.LogError($"[ResourceTests] GetResourceAllVariants after inject (exact type) failed");
            }

            //test GetResources non-type-exact (should get one rendertexture, 1-2 texture2d)
            {
                //not a great test but it'll do for now

                var injectedTexRender2 = new RenderTexture(64, 64, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                rm.AddResource("Modules/TestModule/TypeTest/texture3", injectedTexRender2, ResourcePriority.Explicit);

                Texture[] texArray = rm.GetResources<Texture>("Modules/TestModule/TypeTest", false);

                if (texArray.Contains(injectedTexRender2) && texArray.Length == 3)
                    Debug.Log($"[ResourceTests] GetResources after inject ok");
                else
                    Debug.LogError($"[ResourceTests] GetResources after inject failed");
            }

            //test redirection (single asset)
            {
                TextAsset ta = rm.GetResource<TextAsset>("Modules/TestModule/RedirectTest1/Redirected", false);
                string expected = "REDIRECTED";

                if (ta != null)
                {
                    if (ta.text?.Equals(expected, StringComparison.Ordinal) ?? false)
                        Debug.Log($"[ResourceTests] GetResources of redirected resource ok (expected {expected}, got {ta.text})");
                    else
                        Debug.LogError($"[ResourceTests] GetResources of redirected resource failed (expected {expected}, got {ta.text})");
                }
                else
                    Debug.LogError($"[ResourceTests] GetResources of redirected resource failed (null)");
            }

            //test redirection (folder)

            //test redirection (all variants)


        }


    }
}
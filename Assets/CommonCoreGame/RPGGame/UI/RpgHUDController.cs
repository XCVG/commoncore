using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.Messaging;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.StringSub;
using CommonCore.UI;
using CommonCore.World;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.RpgGame.UI
{
    public class RpgHUDController : BaseHUDController
    {
        [Header("Top Bar")]
        public Text TargetText;
        public Slider TargetHealthLeft;
        public Slider TargetHealthRight;

        [Header("Left Bar")]
        public Slider HealthSlider;
        public Text HealthText;
        public Image HealthFillArea;

        public Slider ShieldSlider;
        public Text ShieldText;
        public Image ShieldFillArea;

        public Slider EnergySlider;
        public Text EnergyText;
        public Image EnergyFillArea;

        public Slider MagicSlider;
        public Text MagicText;
        public Image MagicFillArea;

        [Header("Right Bar")]
        public Text RightWeaponText;
        public Text RightAmmoText;
        public Text RightAmmoReserveText;
        public Text RightAmmoTypeText;

        [Header("Misc")]
        public Canvas Canvas;
        public GameObject HudRoot;
        public GameObject SubtitlesRoot;
        public Image Crosshair;
        public Image HitIndicator;
        public Image DamageFadeOverlay;
        public Image ShieldFadeOverlay;

        [Header("Options")]
        public float HealthbarFlashTime;
        public Color HealthbarFlashColor;
        public float EnergyBarFlashTime;
        public Color EnergyBarFlashColor;
        public float ShieldBarFlashTime;
        public Color ShieldBarFlashColor;
        public float MagicBarFlashTime;
        public Color MagicBarFlashColor;

        public float HitIndicatorHoldTime;
        public float HitIndicatorFadeTime;

        [Header("Damage Fade")]
        public float DamageFadeMin;
        public float DamageFadeMax;
        public float DamageFadeFactor;
        public float DamageFadeRate;
        public Color DamageFadeHealthColor = Color.red;

        [Header("Shields Fade")]
        public float ShieldFadeMin;
        public float ShieldFadeMax;
        public float ShieldFadeFactor;
        public float ShieldFadeRate;
        public Color ShieldFadeColor = Color.cyan;

        //local state is, as it turns out, unavoidable
        private string OverrideTarget = null;


        private float? LastTriggeredHealthFraction = null;
        private float LastFrameHealthFraction = 1;
        private Color? HealthOriginalColor = null;
        private Coroutine HealthFlashCoroutine = null;

        private Color? ShieldOriginalColor = null;
        private Coroutine ShieldFlashCoroutine = null;

        private Color? EnergyOriginalColor = null;
        private Coroutine EnergyFlashCoroutine = null;

        private Color? MagicOriginalColor = null;
        private Coroutine MagicFlashCoroutine = null;

        private float DamageFadeIntensityTarget = 0;
        private float ShieldFadeIntensityTarget = 0;

        private Coroutine HitIndicatorFlashCoroutine = null;

        protected override void Start()
        {
            base.Start();

            LastTriggeredHealthFraction = GameState.Instance.PlayerRpgState.HealthFraction; //okayish
            LastFrameHealthFraction = LastTriggeredHealthFraction.Value;
            UpdateStatusDisplays();
            UpdateWeaponDisplay();

            ClearTarget();
        }
        
        protected override void Update()
        {
            //this is all slow and dumb and temporary... which means it'll probably be untouched until Ferelden
            base.Update();

            UpdateVisibility();
            
            UpdateStatusDisplays();
            UpdateWeaponDisplay(); //ugly but oh well
            UpdateDamageFade();            
            LastFrameHealthFraction = GameState.Instance.PlayerRpgState.HealthFraction;
            UpdateShieldFade();
        }

        protected override bool HandleMessage(QdmsMessage message)
        {
            if(base.HandleMessage(message))
            {
                return true;
            }            
            else if(message is ConfigChangedMessage)
            {
                SetFadeVisibility();
                SetCrosshairOverride();
                return true;
            }
            else if(message is QdmsKeyValueMessage kvmessage)
            {
                switch (kvmessage.Flag)
                {
                    case "RpgChangeWeapon":
                        if(kvmessage.GetValue<CharacterModel>("CharacterModel").IsPlayer)
                            UpdateWeaponDisplay();
                        break;
                    case "PlayerHasTarget":
                        SetTargetMessage(kvmessage.GetValue<string>("Target"));
                        break;
                    case "PlayerTookDamage":
                        {
                            float damageToShields = kvmessage.GetValue<float>("DamageToShields");
                            if (damageToShields > 0)
                                HandleShieldsHit(damageToShields);
                        }
                        break;
                    case "RpgBossHealthUpdate":
                        UpdateTargetOverrideHealth(kvmessage.GetValue<string>("Target"), kvmessage.GetValue<float>("Health"));
                        break;
                    case "RpgBossAwake":
                        SetTargetOverride(kvmessage.GetValue<string>("Target"));
                        break;
                    case "RpgBossDead":
                        ClearTargetOverride(kvmessage.GetValue<string>("Target"));
                        break;
                }

                return true; //probably the wrong spot
            }
            else if(message is QdmsFlagMessage flagmessage)
            {
                switch (flagmessage.Flag)
                {                    
                    case "WepReloading":
                    case "WepFired":
                        //WeaponReady = false;
                        UpdateWeaponDisplay();
                        break;
                    case "WepReady":
                    case "WepReloaded":
                        //WeaponReady = true;
                        UpdateWeaponDisplay();
                        break;
                    case "PlayerShieldsRecharging":
                        HandleShieldsRecharging();
                        break;
                    case "PlayerShieldsLost":
                        HandleShieldsLost();
                        break;
                    case "PlayerChangeView":
                        SetCrosshair(message);
                        break;
                    case "PlayerClearTarget":
                        ClearTarget();
                        break;
                    case "PlayerHitTarget":
                        FlashHitIndicator();
                        break;
                    case "RpgInsufficientMagic":
                        FlashMagicBar();
                        break;
                    case "RpgInsufficientEnergy":
                        FlashEnergyBar();
                        break;
                    case "RpgQuestStarted":
                    case "RpgQuestEnded":
                        AddQuestMessage(message);
                        break;
                    case "HudEnableCrosshair":
                        Crosshair.enabled = true;
                        break;
                    case "HudDisableCrosshair":
                        Crosshair.enabled = false;
                        break;
                }

                return true;
            }

            return false;

        }

        private void SetCrosshairOverride()
        {
            var cs = ConfigState.Instance.GetGameplayConfig().Crosshair;
            switch (cs)
            {
                case CrosshairState.Always:
                    Crosshair.enabled = true;
                    break;
                case CrosshairState.Never:
                    Crosshair.enabled = false;
                    break;
            }
        }

        private void SetCrosshair(QdmsMessage message)
        {
            //we actually don't care much if this fails
            //it'll throw an ugly exception but won't break anything

            //I think this is redundant now (?)

            var newView = ((QdmsKeyValueMessage)(message)).GetValue<PlayerViewType>("ViewType");
            if (newView == PlayerViewType.ForceFirst || newView == PlayerViewType.PreferFirst)
                Crosshair.gameObject.SetActive(true);
            else if(newView == PlayerViewType.ForceThird || newView == PlayerViewType.PreferThird)
                Crosshair.gameObject.SetActive(false);
            else
                Crosshair.gameObject.SetActive(false);
        }

        private void UpdateDamageFade()
        {
            if (!ConfigState.Instance.FlashEffects || !ConfigState.Instance.GetGameplayConfig().FullscreenDamageIndicator)
                return;

            Color damageFadeCurrentColor = DamageFadeOverlay.color;
            float damageFadeCurrentIntensity = damageFadeCurrentColor.a;
            if (DamageFadeIntensityTarget > 0 || (DamageFadeIntensityTarget == 0 && damageFadeCurrentIntensity > 0))
            {
                //make it a more intense red if we keep taking damage
                //if(DamageFadeIntensityTarget > 0)
                {
                    float healthLost = LastFrameHealthFraction - GameState.Instance.PlayerRpgState.HealthFraction;
                    if (!Mathf.Approximately(healthLost, 0))
                    {
                        float extraIntensity = Mathf.Clamp(DamageFadeFactor * healthLost, 0, DamageFadeMax);
                        DamageFadeIntensityTarget = Mathf.Clamp(DamageFadeIntensityTarget + extraIntensity, DamageFadeMin, DamageFadeMax);
                    }
                }

                //fade toward target
                float sign = Mathf.Sign(DamageFadeIntensityTarget - damageFadeCurrentIntensity);
                float newIntensity = Mathf.Clamp(damageFadeCurrentIntensity + sign * DamageFadeRate * Time.deltaTime, 0, 1);
                DamageFadeOverlay.color = new Color(damageFadeCurrentColor.r, damageFadeCurrentColor.g, damageFadeCurrentColor.b, newIntensity);

                //start the fade back
                if (DamageFadeIntensityTarget > 0 && newIntensity >= DamageFadeIntensityTarget)
                    DamageFadeIntensityTarget = 0;
            }            
                    
        }

        private void StartDamageFade(float healthLost)
        {
            //note that healthlost is positive and fractional
            DamageFadeOverlay.color = new Color(DamageFadeHealthColor.r, DamageFadeHealthColor.g, DamageFadeHealthColor.b, DamageFadeOverlay.color.a);
            DamageFadeIntensityTarget = Mathf.Clamp(DamageFadeFactor * healthLost, DamageFadeMin, DamageFadeMax);
            //Debug.LogWarning("Set DamageFadeIntensityTarget to " + DamageFadeIntensityTarget);
        }

        private void UpdateShieldFade()
        {
            if (!ConfigState.Instance.FlashEffects || !ConfigState.Instance.GetGameplayConfig().FullscreenDamageIndicator)
                return;
 
            Color fadeCurrentColor = ShieldFadeOverlay.color;
            float fadeCurrentIntensity = fadeCurrentColor.a;
            if (ShieldFadeIntensityTarget > 0 || (ShieldFadeIntensityTarget == 0 && fadeCurrentIntensity > 0))
            {
                //fade toward target
                float sign = Mathf.Sign(ShieldFadeIntensityTarget - fadeCurrentIntensity);
                float newIntensity = Mathf.Clamp(fadeCurrentIntensity + sign * ShieldFadeRate * Time.deltaTime, 0, 1);
                ShieldFadeOverlay.color = new Color(fadeCurrentColor.r, fadeCurrentColor.g, fadeCurrentColor.b, newIntensity);

                //start the fade back
                if (ShieldFadeIntensityTarget > 0 && newIntensity >= ShieldFadeIntensityTarget)
                    ShieldFadeIntensityTarget = 0;
            }
        }

        private void StartShieldFade(float shieldsLost)
        {
            ShieldFadeOverlay.color = new Color(ShieldFadeColor.r, ShieldFadeColor.g, ShieldFadeColor.b, ShieldFadeOverlay.color.a);
            ShieldFadeIntensityTarget = Mathf.Clamp(ShieldFadeFactor * shieldsLost, ShieldFadeMin, ShieldFadeMax);
        }

        private void SetFadeVisibility()
        {
            if (!ConfigState.Instance.FlashEffects)
            {
                Color oldColor = DamageFadeOverlay.color;
                DamageFadeOverlay.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0);

                Color oldShieldColor = ShieldFadeOverlay.color;
                ShieldFadeOverlay.color = new Color(oldShieldColor.r, oldShieldColor.g, oldShieldColor.b, 0);
            }
        }

        private void UpdateStatusDisplays()
        {
            var player = GameState.Instance.PlayerRpgState;

            HealthText.text = Mathf.Max(0, player.Health).ToString("f0");
            if (LastTriggeredHealthFraction.HasValue && LastTriggeredHealthFraction.Value < player.HealthFraction && HealthFlashCoroutine == null)
                LastTriggeredHealthFraction = player.HealthFraction; //handle healing
            float healthLost = (LastTriggeredHealthFraction ?? HealthSlider.value) - player.HealthFraction;
            if (healthLost > GameParams.DamageFlashThreshold)
            {
                FlashHealthBar();
                StartDamageFade(healthLost);
            }
            HealthSlider.value = player.HealthFraction;

            EnergyText.text = player.Energy.ToString("f0");
            EnergySlider.value = player.EnergyFraction;

            if (player.DerivedStats.ShieldParams.MaxShields > 0)
            {
                ShieldText.text = player.Shields.ToString("f0");
                ShieldSlider.value = player.ShieldsFraction;
                //flashing is handled by messages for shields, not the stupid hacky shit health uses
            }
            else
            {
                ShieldText.text = "";
                ShieldSlider.value = 0;
            }

            if (player.DerivedStats.MaxMagic > 0)
            {
                if(MagicText != null)
                    MagicText.text = player.Magic.ToString("f0");
                if(MagicSlider != null)
                    MagicSlider.value = player.MagicFraction;
                //flashing is handled by messages for magic, not the stupid hacky shit health uses
            }
            else
            {
                if (MagicText != null)
                    MagicText.text = "";
                if (MagicSlider != null)
                    MagicSlider.value = 0;
            }

        }

        private void HandleShieldsHit(float damageToShields)
        {            
            if(damageToShields > GameParams.DamageFlashThreshold)
            {
                //flash the bar, flash the screen
                FlashShieldBar();
                StartShieldFade(damageToShields);
            }
        }

        private void HandleShieldsLost()
        {
            //TODO flash the bar frantically until recharging starts
            //(probably use a separate coroutine for this)
        }

        private void HandleShieldsRecharging()
        {
            //clear bar flash
            if(ShieldFlashCoroutine != null)
            {
                StopCoroutine(ShieldFlashCoroutine);
                ShieldFlashCoroutine = null;

                ShieldFillArea.color = ShieldOriginalColor.Value;
            }

            //TODO clear flashing, highlight bar (?)
        }

        private void FlashHitIndicator()
        {
            var gameplayConfig = ConfigState.Instance.GetGameplayConfig();

            if (gameplayConfig.HitIndicatorsAudio)
            {
                AudioPlayer.Instance.PlayUISound("HitIndicator");
            }

            if (gameplayConfig.HitIndicatorsVisual)
            {
                if (HitIndicatorFlashCoroutine != null)
                    StopCoroutine(HitIndicatorFlashCoroutine);

                HitIndicator.color = new Color(HitIndicator.color.r, HitIndicator.color.g, HitIndicator.color.b, 1f);

                HitIndicatorFlashCoroutine = StartCoroutine(FlashHitIndicatorCoroutine());
            }
        }

        private IEnumerator FlashHitIndicatorCoroutine()
        {
            yield return new WaitForSeconds(HitIndicatorHoldTime);

            for(float elapsed = 0; elapsed < HitIndicatorFadeTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / HitIndicatorFadeTime;

                HitIndicator.color = new Color(HitIndicator.color.r, HitIndicator.color.g, HitIndicator.color.b, 1f - ratio);

                yield return null;
            }

            HitIndicator.color = new Color(HitIndicator.color.r, HitIndicator.color.g, HitIndicator.color.b, 0);

            HitIndicatorFlashCoroutine = null;
        }

        private void FlashShieldBar()
        {
            if (!ShieldOriginalColor.HasValue)
                ShieldOriginalColor = ShieldFillArea.color;

            if (ShieldFlashCoroutine != null)
                return;

            ShieldFlashCoroutine = StartCoroutine(FlashShieldBarCoroutine());
        }

        private IEnumerator FlashShieldBarCoroutine()
        {

            yield return null;

            float fadeHalfTime = ShieldBarFlashTime / 2f;

            ShieldFillArea.color = ShieldOriginalColor.Value;

            //fade to final color
            for (float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(ShieldOriginalColor.Value, ShieldBarFlashColor, ratio);
                ShieldFillArea.color = c;

                yield return null;
            }

            ShieldFillArea.color = ShieldBarFlashColor;
            yield return null;

            //fade back to original color

            for (float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(ShieldBarFlashColor, ShieldOriginalColor.Value, ratio);
                ShieldFillArea.color = c;

                yield return null;
            }

            ShieldFillArea.color = ShieldOriginalColor.Value;
            ShieldFlashCoroutine = null;
        }

        private void FlashHealthBar()
        {
            if (!HealthOriginalColor.HasValue)
                HealthOriginalColor = HealthFillArea.color;

            //Debug.Log("FlashHealthBar");

            if (HealthFlashCoroutine != null)
                return;
                //StopCoroutine(HealthFlashCoroutine);

            HealthFlashCoroutine = StartCoroutine(FlashHealthBarCoroutine());
        }

        private IEnumerator FlashHealthBarCoroutine()
        {
            float healthOriginalValue = HealthSlider.value;

            yield return null;

            float fadeHalfTime = HealthbarFlashTime / 2f;

            HealthFillArea.color = HealthOriginalColor.Value;

            //fade to final color
            for(float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(HealthOriginalColor.Value, HealthbarFlashColor, ratio);
                HealthFillArea.color = c;

                yield return null;
            }

            HealthFillArea.color = HealthbarFlashColor;
            yield return null;

            //fade back to original color

            for (float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(HealthbarFlashColor, HealthOriginalColor.Value, ratio);
                HealthFillArea.color = c;

                yield return null;
            }

            HealthFillArea.color = HealthOriginalColor.Value;
            LastTriggeredHealthFraction = Mathf.Max(healthOriginalValue, GameState.Instance.PlayerRpgState.HealthFraction);
            HealthFlashCoroutine = null;

        }

        private void FlashEnergyBar()
        {
            //flash the energy bar
            if (!EnergyOriginalColor.HasValue)
                EnergyOriginalColor = EnergyFillArea.color;

            if (EnergyFlashCoroutine != null)
                return;

            EnergyFlashCoroutine = StartCoroutine(FlashEnergyBarCoroutine());
        }

        private IEnumerator FlashEnergyBarCoroutine()
        {
            yield return null;

            float fadeHalfTime = EnergyBarFlashTime / 2f;

            EnergyFillArea.color = EnergyOriginalColor.Value;

            //fade to final color
            for (float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(EnergyOriginalColor.Value, EnergyBarFlashColor, ratio);
                EnergyFillArea.color = c;

                yield return null;
            }

            EnergyFillArea.color = EnergyBarFlashColor;
            yield return null;

            //fade back to original color

            for (float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(EnergyBarFlashColor, EnergyOriginalColor.Value, ratio);
                EnergyFillArea.color = c;

                yield return null;
            }

            EnergyFillArea.color = EnergyOriginalColor.Value;

            EnergyFlashCoroutine = null;
        }

        private void FlashMagicBar()
        {
            if (MagicSlider == null || MagicFillArea == null)
                return;

            //flash the energy bar
            if (!MagicOriginalColor.HasValue)
                MagicOriginalColor = MagicFillArea.color;

            if (MagicFlashCoroutine != null)
                return;

            MagicFlashCoroutine = StartCoroutine(FlashMagicBarCoroutine());
        }

        private IEnumerator FlashMagicBarCoroutine()
        {
            yield return null;

            float fadeHalfTime = MagicBarFlashTime / 2f;

            MagicFillArea.color = MagicOriginalColor.Value;

            //fade to final color
            for (float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(MagicOriginalColor.Value, MagicBarFlashColor, ratio);
                MagicFillArea.color = c;

                yield return null;
            }

            MagicFillArea.color = MagicBarFlashColor;
            yield return null;

            //fade back to original color

            for (float elapsed = 0; elapsed < fadeHalfTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeHalfTime;

                Color c = Vector4.Lerp(MagicBarFlashColor, MagicOriginalColor.Value, ratio);
                MagicFillArea.color = c;

                yield return null;
            }

            MagicFillArea.color = MagicOriginalColor.Value;

            MagicFlashCoroutine = null;
        }

        private void UpdateWeaponDisplay()
        {
            var player = GameState.Instance.PlayerRpgState;

            //ignore the left weapon even if it exists
            if (player.IsEquipped(EquipSlot.RightWeapon))
            {
                RightWeaponText.text = InventoryModel.GetNiceName(player.Equipped[EquipSlot.RightWeapon].ItemModel);
                if (player.Equipped[EquipSlot.RightWeapon].ItemModel is RangedWeaponItemModel rwim && rwim.UseAmmo)
                {
                    if (rwim.UseMagazine)
                    {
                        RightAmmoText.text = player.AmmoInMagazine[EquipSlot.RightWeapon].ToString();
                        RightAmmoReserveText.text = player.Inventory.CountItem(rwim.AType.ToString()).ToString();
                    }
                    else
                    {
                        RightAmmoText.text = player.Inventory.CountItem(rwim.AType.ToString()).ToString();
                        RightAmmoReserveText.text = "";
                    }


                    RightAmmoTypeText.text = InventoryModel.GetNiceName(InventoryModel.GetModel(rwim.AType.ToString()));
                }
                else
                {
                    RightAmmoText.text = "-";
                    RightAmmoReserveText.text = "-";
                    RightAmmoTypeText.text = "";
                }
            }
            else
            {
                RightWeaponText.text = "No Weapon";
                RightAmmoText.text = "-";
                RightAmmoReserveText.text = "-";
                RightAmmoTypeText.text = "";
            }

        }

        private void UpdateVisibility()
        {
            //TODO handle HideHud but also HideSubtitles

            if(GameState.Instance.PlayerFlags.Contains(PlayerFlags.HideHud))
            {
                if (HudRoot.activeSelf)
                    HudRoot.SetActive(false);
            }
            else
            {
                if (!HudRoot.activeSelf)
                    HudRoot.SetActive(true);
            }

            if (GameState.Instance.PlayerFlags.Contains(PlayerFlags.HideSubtitles))
            {
                if (SubtitlesRoot.activeSelf)
                    SubtitlesRoot.SetActive(false);
            }
            else
            {
                if (!SubtitlesRoot.activeSelf)
                    SubtitlesRoot.SetActive(true);
            }
        }

        private void AddQuestMessage(QdmsMessage message)
        {
            var qMessage = message as QdmsKeyValueMessage;
            if (qMessage == null)
            {
                return;
            }                
            else if(qMessage.Flag == "RpgQuestStarted")
            {
                var qRawName = qMessage.GetValue<string>("Quest");
                var qDef = QuestModel.GetDef(qRawName);
                string questName = qDef == null ? qRawName : qDef.NiceName;
                AppendHudMessage(string.Format("Quest Started: {0}", questName));
            }
            else if(qMessage.Flag == "RpgQuestEnded")
            {
                var qRawName = qMessage.GetValue<string>("Quest");
                var qDef = QuestModel.GetDef(qRawName);
                string questName = qDef == null ? qRawName : qDef.NiceName;
                AppendHudMessage(string.Format("Quest Ended: {0}", questName));
            }
        }

        //handle target text, override target text, health bar

        private void ClearTarget()
        {
            TargetText.text = string.Empty;

            if (!string.IsNullOrEmpty(OverrideTarget))
                TargetText.text = OverrideTarget;
        }

        private void SetTargetMessage(string message)
        {
            if (!string.IsNullOrEmpty(OverrideTarget))
                return;

            TargetText.text = message;
        }

        private void SetTargetOverride(string overrideTarget)
        {
            OverrideTarget = overrideTarget;
            TargetText.text = overrideTarget;

            TargetHealthLeft.gameObject.SetActive(true);
            TargetHealthRight.gameObject.SetActive(true);
            TargetHealthLeft.value = 1;
            TargetHealthRight.value = 1;
        }

        private void UpdateTargetOverrideHealth(string overrideTarget, float health)
        {
            if (OverrideTarget == null || OverrideTarget != overrideTarget)
            {
                Debug.LogWarning($"[{nameof(RpgHUDController)}] Updated override target health for a different target than expected (old: \"{OverrideTarget}\", new: \"{overrideTarget}\")");
                SetTargetOverride(overrideTarget);                
            }

            TargetHealthLeft.value = health;
            TargetHealthRight.value = health;
        }

        private void ClearTargetOverride(string overrideTarget)
        {
            if (OverrideTarget == null || OverrideTarget != overrideTarget)
            {
                Debug.LogWarning($"[{nameof(RpgHUDController)}] Cleared override target for a different target than expected (old: \"{OverrideTarget}\", new: \"{overrideTarget}\")");
            }

            OverrideTarget = null;
            ClearTarget();

            TargetHealthLeft.gameObject.SetActive(false);
            TargetHealthRight.gameObject.SetActive(false);
        }
    }
}
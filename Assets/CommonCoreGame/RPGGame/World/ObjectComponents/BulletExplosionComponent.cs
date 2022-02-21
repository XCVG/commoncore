using CommonCore.World;
using CommonCore.RpgGame.Rpg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.World;
using CommonCore;
using CommonCore.State;

namespace CommonCore.RpgGame.World //here because we need factions
{
	/// <summary>
	/// Somewhat hacky script for making bullets go boom
	/// </summary>
	public class BulletExplosionComponent : MonoBehaviour
	{
		[SerializeField]
		private BulletScript BulletScript = null;

		[Header("Explosion Parameters")]
		public float Damage = 10f;
		public float Radius = 5f;
		public bool UseFalloff = true;
		public string HitPuff = string.Empty;

		[Header("Detonation Parameters")]
		public bool DetonateOnWorldHit = true;
		public bool DetonateOnDespawn = false;

		[Header("Proximity detonation")]
		public bool EnableProximityDetonation = true;
		public float ProximityRadius = 3f;
		public bool UseFactions = false;
		[Tooltip("If enabled, will only detonate if it is beside or past the target")]
		public bool UseTangentHack = false;

		[Header("Effect Parameters")]
		public string ExplosionEffect = null;
		public bool ApplyVelocityToEffect = false;		

		private bool Triggered = false;

		private void Start()
		{
			if (BulletScript == null)
				BulletScript = GetComponent<BulletScript>();

		}


		private void Update()
		{
			//explode in close proximity to enemies
			if (EnableProximityDetonation)
			{
				if (BulletScript.HitInfo.Originator == null)
					return; //a bit of a hack; hitinfo not set up yet

				var hits = WorldUtils.OverlapSphereAttackHit(transform.position, ProximityRadius, true, false, false, BulletScript.HitInfo.Originator);
				IList<HitInfo> goodHits;
				if (UseFactions)
				{
					string originatorFaction = null;
					if (!string.IsNullOrEmpty(BulletScript.HitInfo.OriginatorFaction))
						originatorFaction = BulletScript.HitInfo.OriginatorFaction;
					else if (BulletScript.HitInfo.Originator is PlayerController)
						originatorFaction = PredefinedFaction.Player.ToString();
					else if (BulletScript.HitInfo.Originator is ActorController ac)
						originatorFaction = ac.Faction;

					if (string.IsNullOrEmpty(originatorFaction))
						return;

					goodHits = new List<HitInfo>();
					foreach (var hit in hits)
					{
						string targetFaction = PredefinedFaction.None.ToString();
						if (hit.Controller != null)
						{
							if (hit.Controller is ActorController ac)
								targetFaction = ac.Faction;
							else if (hit.Controller is PlayerController)
								targetFaction = PredefinedFaction.Player.ToString();
						}

						var relation = GameState.Instance.FactionState.GetRelation(originatorFaction, targetFaction);
						if (relation == FactionRelationStatus.Hostile)
							goodHits.Add(hit);
					}
				}
				else
				{
					goodHits = hits;
				}

				if (UseTangentHack)
				{
					var possibleHits = goodHits;
					goodHits = new List<HitInfo>();
					foreach (var pHit in possibleHits)
					{
						Vector3 vecBulletToTarget = pHit.HitPoint - transform.position;
						Vector3 dirBulletToTarget = vecBulletToTarget.normalized;
						float dirDotFacing = Vector3.Dot(dirBulletToTarget, transform.forward);
						if (dirDotFacing <= 0)
							goodHits.Add(pHit);
					}
				}

				if (goodHits.Count > 0)
				{
					HitPuffScript.SpawnHitPuff(BulletScript.HitInfo.HitPuff, transform.position, BulletScript.HitInfo.HitMaterial);

					Destroy(this.gameObject);
				}
			}
		}

		private void OnDestroy()
		{
			if (Triggered || !enabled)
				return;

			if(BulletScript.DestroyType == BulletScriptDestroyType.HitDamageable || (BulletScript.DestroyType == BulletScriptDestroyType.HitWorld && DetonateOnWorldHit) || DetonateOnDespawn)
				Explode();
		}

		private void Explode()
		{
			Triggered = true;

			//do radius damage

			var bulletHitInfo = BulletScript.HitInfo;

			ActorHitInfo hitInfo = new ActorHitInfo(Damage, 0, bulletHitInfo.DamageType, (int)DefaultDamageEffectors.Explosion, false, 0, 0, bulletHitInfo.Originator, bulletHitInfo.OriginatorFaction, HitPuff, null, bulletHitInfo.HitFlags);
			//TODO we copy flags, should we also copy ExtraFlags and ExtraData?

			WorldUtils.RadiusDamage(transform.position, Radius, UseFalloff, true, false, false, false, hitInfo);

			//TODO physics

			if(!string.IsNullOrEmpty(ExplosionEffect))
            {
				var explosionEffect = WorldUtils.SpawnEffect(ExplosionEffect, transform.position, transform.rotation, null, false);
				if(explosionEffect != null && ApplyVelocityToEffect)
                {
					var rb = BulletScript.gameObject.GetComponent<Rigidbody>();
					var erb = explosionEffect.GetComponent<Rigidbody>();
					if(rb != null && erb != null)
                    {
						erb.velocity = rb.velocity;
                    }
                }
            }

			
		}
	}
}
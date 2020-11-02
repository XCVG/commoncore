using CommonCore.World;
using CommonCore.RpgGame.Rpg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.World;
using CommonCore;

namespace CommonCore.Experimental
{
	/// <summary>
	/// Somewhat hacky script for making bullets go boom
	/// </summary>
	public class BulletExplosionComponentEx : MonoBehaviour
	{
		[SerializeField]
		private BulletScript BulletScript = null;

		[SerializeField, Header("Explosion Parameters")]
		private float Damage = 10f;
		[SerializeField]
		private float Radius = 5f;
		[SerializeField]
		private bool UseFalloff = true;
		[SerializeField]
		private string HitPuff = string.Empty;

		[SerializeField, Header("Proximity detonation")]
		private bool EnableProximityDetonation = true;
		[SerializeField]
		private float ProximityRadius = 3f;
		[SerializeField]
		private bool UseFactions = false;
		[SerializeField, Tooltip("If enabled, will only detonate if it is beside or past the target")]
		private bool UseTangentHack = false;

		private bool Triggered = false;

		private void Start()
		{
			if (BulletScript == null)
				BulletScript = GetComponent<BulletScript>();

		}


		private void Update()
		{
			//explode in close proximity to enemies
			if(EnableProximityDetonation)
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
						if(hit.Controller != null)
						{
							if (hit.Controller is ActorController ac)
								targetFaction = ac.Faction;
							else if (hit.Controller is PlayerController)
								targetFaction = PredefinedFaction.Player.ToString();
						}

						var relation = FactionModel.GetRelation(originatorFaction, targetFaction);
						if (relation == FactionRelationStatus.Hostile)
							goodHits.Add(hit);
					}
				}
				else
				{
					goodHits = hits;
				}

				if(UseTangentHack)
				{
					var possibleHits = goodHits;
					goodHits = new List<HitInfo>();
					foreach(var pHit in possibleHits)
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
			if (Triggered)
				return;

			DoRadiusDamage();
		}

		private void DoRadiusDamage()
		{
			//do radius damage

			var bulletHitInfo = BulletScript.HitInfo;

			ActorHitInfo hitInfo = new ActorHitInfo(Damage, 0, bulletHitInfo.DamageType, (int)DefaultDamageEffectors.Explosion, false, 0, 0, bulletHitInfo.Originator, bulletHitInfo.OriginatorFaction, HitPuff, null, bulletHitInfo.HitFlags);
            //TODO we copy flags, should we also copy ExtraFlags and ExtraData?

			WorldUtils.RadiusDamage(transform.position, Radius, UseFalloff, true, false, false, false, hitInfo);

			Triggered = true;
		}
	}
}
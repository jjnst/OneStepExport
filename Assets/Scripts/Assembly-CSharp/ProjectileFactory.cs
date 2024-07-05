using System;
using System.Globalization;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class ProjectileFactory
{
	public static ProjectileFactory I;

	private BC ctrl;

	private DeckCtrl deCtrl;

	private Being being;

	private Projectile proj;

	public ProjectileFactory()
	{
		I = this;
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
	}

	public Projectile CreateShot(SpellObject spellObj, Tile tile, bool isLocal = false, int pelletIndex = 0, float pelletInterval = 1f)
	{
		proj = CreateProjectile(spellObj, tile.x, tile.y, spellObj.shotDuration, spellObj.damage, spellObj.animShot, spellObj.shotSound, isLocal, spellObj.hitboxWidth, spellObj.hitboxHeight, false, spellObj.hitSound, spellObj.animHit, pelletIndex, pelletInterval);
		ApplyVelocityAndOffset(spellObj, proj, spellObj.shotVelocity, spellObj.shotVelocityY);
		return proj;
	}

	public Projectile CreateShot(SpellObject spellObj, int tileX, int tileY, bool isLocal = false)
	{
		proj = CreateProjectile(spellObj, tileX, tileY, spellObj.shotDuration, spellObj.damage, spellObj.animShot, spellObj.shotSound, isLocal, spellObj.hitboxWidth, spellObj.hitboxHeight, false, spellObj.hitSound, spellObj.animHit);
		ApplyVelocityAndOffset(spellObj, proj, spellObj.shotVelocity, spellObj.shotVelocityY);
		return proj;
	}

	public Projectile CreateCustomShot(SpellObject spellObj, Tile tile, bool isLocal = false, float duration = 0f, int damage = 0, float shotVelocity = 0f, string animName = null, string spawnSound = null, int hitboxWidth = 1, int hitboxHeight = 1, string hitSound = null, string animHit = null)
	{
		proj = CreateProjectile(spellObj, tile.x, tile.y, duration, damage, animName, spawnSound, isLocal, hitboxWidth, hitboxHeight, false, hitSound, animHit);
		ApplyVelocityAndOffset(spellObj, proj, shotVelocity, 0f);
		return proj;
	}

	public Projectile CreateCustomShot(SpellObject spellObj, int tileX, int tileY, bool isLocal = false, float duration = 0f, int damage = 0, float shotVelocity = 0f, string animName = null, string spawnSound = null, int hitboxWidth = 1, int hitboxHeight = 1, string hitSound = null, string animHit = null)
	{
		proj = CreateProjectile(spellObj, tileX, tileY, duration, damage, animName, spawnSound, isLocal, hitboxWidth, hitboxHeight, false, hitSound, animHit);
		ApplyVelocityAndOffset(spellObj, proj, shotVelocity, 0f);
		return proj;
	}

	public Projectile ApplyVelocityAndOffset(SpellObject spellObj, Projectile proj, float shotVelocityX, float shotVelocityY)
	{
		if (spellObj.shotVelocity != 0f && shotVelocityX != 0f)
		{
			proj.rBody.velocity = spellObj.being.transform.TransformDirection(Vector3.right * shotVelocityX);
		}
		if (spellObj.shotVelocityY != 0f && shotVelocityY != 0f)
		{
			proj.rBody.velocity += Vector2.up * shotVelocityY;
		}
		if (spellObj.hitboxOffset.x != 0f || spellObj.hitboxOffset.y != 0f)
		{
			proj.col.offset += new Vector2(spellObj.hitboxOffset.x * 40f, spellObj.hitboxOffset.y * 25f);
		}
		return proj;
	}

	public Projectile CreateLerper(SpellObject spellObj, int x, int y, bool isLocal = false)
	{
		Tile tile = null;
		tile = ((!isLocal) ? spellObj.being.battleGrid.grid[x, y] : spellObj.being.TileLocal(x, y));
		proj = CreateLerper(spellObj, tile);
		return proj;
	}

	private void SetLerperAttributes(Projectile proj, float lerpTime, float lerpDelay, Direction direction, MovPattern movPattern, SpellObject spellObj, Tile tile)
	{
		proj.mov.lerpTime = lerpTime;
		proj.lerpDelay = lerpDelay;
		proj.mov.direction = direction;
		proj.mov.movement = movPattern;
		proj.mov.battleGrid = spellObj.being.battleGrid;
		proj.mov.currentTile = tile;
		proj.mov.startTile = tile;
		proj.mov.endTile = tile;
	}

	public Projectile CreateLerper(SpellObject spellObj, Tile tile, bool isLocal = false)
	{
		proj = CreateProjectile(spellObj, tile.x, tile.y, spellObj.shotDuration, spellObj.damage, spellObj.animShot, spellObj.shotSound, isLocal, spellObj.hitboxWidth, spellObj.hitboxHeight);
		SetLerperAttributes(proj, spellObj.shotVelocity, float.Parse(spellObj.Param("lerpDelay"), NumberStyles.Float, CultureInfo.InvariantCulture), (Direction)Enum.Parse(typeof(Direction), spellObj.Param("lerpDirection")), (MovPattern)Enum.Parse(typeof(MovPattern), spellObj.Param("lerpPattern")), spellObj, tile);
		if (spellObj.HasParam("destroyIfNoTile"))
		{
			proj.mov.destroyIfNoTile = bool.Parse(spellObj.Param("destroyIfNoTile"));
		}
		if (spellObj.HasParam("hovering"))
		{
			proj.mov.hovering = bool.Parse(spellObj.Param("hovering"));
		}
		else
		{
			proj.mov.hovering = true;
		}
		return proj;
	}

	public Projectile CreateCustomLerper(SpellObject spellObj, Tile tile, MovPattern movement = MovPattern.None, float lerpTime = 0.1f, float lerpDelay = 0f, bool isLocal = false, float duration = 0f, int damage = 0, string animName = null, string soundName = null, string hitSound = null, string animHit = null)
	{
		proj = CreateProjectile(spellObj, tile.x, tile.y, duration, damage, animName, soundName, isLocal, spellObj.hitboxWidth, spellObj.hitboxHeight, false, hitSound, animHit);
		SetLerperAttributes(proj, lerpTime, lerpDelay, Direction.None, movement, spellObj, tile);
		return proj;
	}

	public Projectile CreateLerperEffect(SpellObject spellObj, Tile tile, MovPattern movement = MovPattern.None, float lerpTime = 0.1f, float lerpDelay = 0f, bool isLocal = false, float duration = 0f, string animName = null, string soundName = null)
	{
		proj = CreateProjectile(spellObj, tile.x, tile.y, duration, 0f, animName, soundName, isLocal, spellObj.hitboxWidth, spellObj.hitboxHeight, true);
		SetLerperAttributes(proj, lerpTime, lerpDelay, Direction.None, movement, spellObj, tile);
		return proj;
	}

	public Projectile CreateAnimObj(SpellObject spellObj, Tile tile, float duration, bool isLocal = false, float speed = 0f, int pelletIndex = 0, float pelletInterval = 1f)
	{
		return CreateCustomEffect(spellObj, tile, duration, spellObj.animObj, isLocal, speed, null, pelletIndex, pelletInterval);
	}

	public Projectile CreateCastEffect(SpellObject spellObj, int x, int y, bool isLocal = false, float speed = 0f, int pelletIndex = 0, float pelletInterval = 1f, bool setToGunpoint = false)
	{
		return CreateCustomEffect(spellObj, x, y, spellObj.castDuration, spellObj.animCast, isLocal, speed, spellObj.castSound, 0, 1f, setToGunpoint);
	}

	private bool SubsequentPellet(int pelletIndex, float pelletInterval)
	{
		if (pelletIndex != 0 && pelletInterval == 0f)
		{
			return true;
		}
		return false;
	}

	public Projectile CreateCastEffect(SpellObject spellObj, Tile tile, bool isLocal = false, float speed = 0f, int pelletIndex = 0, float pelletInterval = 1f, bool setToGunpoint = false)
	{
		return CreateCustomEffect(spellObj, tile, spellObj.castDuration, spellObj.animCast, isLocal, speed, spellObj.castSound, pelletIndex, pelletInterval, setToGunpoint);
	}

	public Projectile CreateBlastEffect(SpellObject spellObj, int x, int y, bool isLocal = false, float speed = 0f, bool setToGunpoint = false)
	{
		return CreateCustomEffect(spellObj, x, y, spellObj.blastDuration, spellObj.animBlast, isLocal, speed, null, 0, 1f, setToGunpoint);
	}

	public Projectile CreateBlastEffect(SpellObject spellObj, Tile tile, bool isLocal = false, float speed = 0f, bool setToGunpoint = false)
	{
		return CreateCustomEffect(spellObj, tile, spellObj.blastDuration, spellObj.animBlast, isLocal, speed, null, 0, 1f, setToGunpoint);
	}

	public Projectile CreateCustomEffect(SpellObject spellObj, Tile tile, float duration, string animMark, bool isLocal = false, float speed = 0f, string soundName = null, int pelletIndex = 0, float pelletInterval = 1f, bool setToGunpoint = false, bool marker = false)
	{
		return CreateCustomEffect(spellObj, tile.x, tile.y, duration, animMark, isLocal, speed, soundName, pelletIndex, pelletInterval, setToGunpoint, marker);
	}

	public Projectile CreateCustomEffect(SpellObject spellObj, int x, int y, float duration, string animMark, bool isLocal = false, float speed = 0f, string soundName = null, int pelletIndex = 0, float pelletInterval = 1f, bool setToGunpoint = false, bool marker = false)
	{
		if (string.IsNullOrEmpty(animMark) && string.IsNullOrEmpty(soundName) && !marker)
		{
			return null;
		}
		Projectile projectile = CreateProjectile(spellObj, x, y, duration, 0f, animMark, soundName, isLocal, 0, 0, true, null, null, pelletIndex, pelletInterval);
		if (setToGunpoint)
		{
			projectile.SetToGunPoint(spellObj.gunPointSetting);
		}
		if (speed != 0f)
		{
			projectile.rBody.velocity = spellObj.being.transform.TransformDirection(Vector3.right * speed);
		}
		return projectile;
	}

	public Projectile CreateWarning(SpellObject spellObj, int x, int y, float duration, bool isLocal = false, float speed = 0f)
	{
		return CreateWarning(spellObj, spellObj.being.battleGrid.grid[x, y], duration, isLocal, speed);
	}

	public Projectile CreateWarning(SpellObject spellObj, Tile tile, float duration, bool isLocal = false, float speed = 0f)
	{
		Projectile projectile = CreateProjectile(spellObj, tile.x, tile.y, duration, 0f, spellObj.animWarning, spellObj.warningSound, isLocal, 0, 0, true);
		if (speed != 0f)
		{
			projectile.rBody.velocity = spellObj.being.transform.TransformDirection(Vector3.right * speed);
		}
		if (string.IsNullOrEmpty(spellObj.animWarning) && ctrl.pvpMode)
		{
			spellObj.animWarning = "WarningDangerC";
		}
		if (string.IsNullOrEmpty(spellObj.animWarning) || spellObj.animWarning.Contains("Danger"))
		{
			projectile.StartWarningTimer(duration, true);
		}
		else
		{
			projectile.StartWarningTimer(duration, false);
		}
		return projectile;
	}

	public Projectile CreateProjectile(SpellObject spellObj, float tileX, float tileY, float duration, float damage, string animName = null, string soundEffect = null, bool isLocal = false, int hitboxWidth = 0, int hitboxHeight = 0, bool isEffect = false, string hitSound = null, string hitAnim = null, int pelletIndex = 0, float pelletInterval = 1f)
	{
		if (spellObj == null)
		{
			return null;
		}
		if (spellObj.being == null)
		{
			return null;
		}
		being = spellObj.being;
		proj = SimplePool.Spawn(ctrl.projectilePrefab, being.transform.position, being.transform.rotation).GetComponent<Projectile>();
		proj.spellObj = spellObj;
		proj.spell = spellObj.spell;
		proj.ctrl = ctrl;
		proj.battleGrid = being.battleGrid;
		proj.battleGrid.currentProjectiles.Add(proj);
		if (spellObj.spawnOffset != Vector2.zero)
		{
			tileX += spellObj.spawnOffset.x * (float)being.FacingDirection();
			tileY += spellObj.spawnOffset.y;
		}
		if (isLocal)
		{
			proj.transform.position = spellObj.being.TileLocal(0).transform.position + new Vector3(tileX * (float)TI.tileWidth * (float)spellObj.being.FacingDirection(), tileY * (float)TI.tileHeight, tileY);
		}
		else if (!isLocal)
		{
			proj.transform.position = spellObj.being.battleGrid.grid[0, 0].transform.position + new Vector3(tileX * (float)TI.tileWidth, tileY * (float)TI.tileHeight, tileY);
		}
		proj.transform.position -= Vector3.forward * 0.1f;
		proj.transform.SetParent(ctrl.transform);
		proj.name = spellObj.itemID;
		proj.tag = "Projectile";
		proj.spriteRenderer.sortingLayerName = "ProjChar";
		if (spellObj.effectLayer)
		{
			proj.spriteRenderer.sortingLayerName = "Effects";
		}
		proj.damage = Mathf.RoundToInt(ctrl.GetAmount(spellObj.damageType, damage, spellObj));
		proj.damage += spellObj.tempDamage;
		proj.damage += spellObj.permDamage;
		if (spellObj.damageType.type != AmountType.Zero)
		{
			proj.damage = Mathf.Clamp(proj.damage, 0, proj.damage);
		}
		proj.pierceShield = spellObj.pierceShield;
		proj.pierceDefense = spellObj.pierceDefense;
		if (!spellObj.tags.Contains(Tag.Weapon) && !spellObj.tags.Contains(Tag.Drone))
		{
			if (proj.damage > 0)
			{
				if ((bool)being.player)
				{
					proj.damage += being.player.spellPower;
				}
				proj.damage += being.GetAmount(Status.SpellPower);
				if (proj.damage < 1)
				{
					proj.damage = 1;
				}
			}
		}
		else if (spellObj.tags.Contains(Tag.Weapon) && proj.damage > 0)
		{
			if ((bool)spellObj.being.player)
			{
				proj.damage += being.player.atkDmg;
			}
			if ((bool)being.GetStatusEffect(Status.AtkDmg))
			{
				proj.damage += Mathf.RoundToInt(being.GetStatusEffect(Status.AtkDmg).amount);
			}
		}
		proj.alignNum = being.alignNum;
		proj.mov.alignNum = being.alignNum;
		proj.destroyOnHit = spellObj.destroyOnHit;
		proj.being = being;
		proj.mov.being = being;
		proj.mov.battleGrid = proj.battleGrid;
		proj.mov.isProjectile = true;
		proj.SetY(spellObj.yVariance);
		if (soundEffect != null && !SubsequentPellet(pelletIndex, pelletInterval))
		{
			S.I.PlayOnce(ctrl.deCtrl.itemMan.GetAudioClip(soundEffect), proj.IsReference());
		}
		if (hitSound != null)
		{
			proj.hitSound = hitSound;
		}
		if (hitAnim != null)
		{
			proj.hitAnim = hitAnim;
		}
		if (animName != null)
		{
			if (deCtrl.itemMan.animations.ContainsKey(animName))
			{
				proj.sprAnim.enabled = false;
				proj.anim.runtimeAnimatorController = ctrl.deCtrl.itemMan.GetAnim(animName);
				if (duration == 0f)
				{
					duration = proj.anim.runtimeAnimatorController.animationClips[0].length;
				}
			}
			else
			{
				proj.anim.enabled = false;
				proj.sprAnim.AssignClip(ctrl.deCtrl.itemMan.GetClip(animName));
			}
		}
		else
		{
			proj.anim.enabled = false;
			proj.sprAnim.enabled = false;
			proj.anim.runtimeAnimatorController = null;
		}
		foreach (StatusApp statusApp in spellObj.statusApps)
		{
			proj.statusList.Add(statusApp);
		}
		proj.onHitTriggerArts = spellObj.onHitTriggerArts;
		spellObj.ClearSpellFlowStatusApps();
		if (spellObj.faceVelocity)
		{
			proj.transform.right = Vector3.zero + new Vector3(spellObj.shotVelocity, spellObj.shotVelocityY, 0f);
		}
		if (isEffect)
		{
			proj.tag = "Effect";
			proj.col.enabled = false;
		}
		else
		{
			if (!string.IsNullOrEmpty(spellObj.lineTracer))
			{
				proj.CreateLineTracer(spellObj.lineTracer);
			}
			int num = hitboxWidth * TI.tileWidth - 30;
			int num2 = hitboxHeight * TI.tileHeight - 20;
			if (hitboxWidth <= 1)
			{
				num = 2;
			}
			if (hitboxHeight <= 1)
			{
				num2 = 2;
			}
			proj.col.size = new Vector2(num, num2);
		}
		being.activeProjectiles.Add(proj);
		proj.Despawn(duration);
		return proj;
	}
}

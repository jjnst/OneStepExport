using System.Collections.Generic;
using SonicBloom.Koreo;

public class ViolettePath : Spell
{
	public BossViolette boss;

	public List<Tile> edxTiles = new List<Tile>();

	public bool pathing = false;

	public int notesPlayed = 0;

	public int missedNotes = 0;

	protected override void Start()
	{
		base.Start();
		boss = being.GetComponent<BossViolette>();
		spellObj.animWarning = "WarningDangerC";
		ResetSong();
		spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.Shield, Target.Hit, int.Parse(boss.spellObjList[6].Param("damagePerNote")), 0f, "", 1f, 0f));
		float amountVar = 0f;
		spellObj.damageType = new AmountApp(ref amountVar, "Zero");
	}

	public void ResetSong()
	{
		edxTiles.Clear();
		edxTiles.Add(boss.battleGrid.Get(new TileApp(Location.Player, Shape.Adjacent, Pattern.Moveable), 1, ctrl.currentPlayer)[0]);
	}

	public void StartPathing()
	{
		pathing = true;
		notesPlayed = 0;
		missedNotes = 0;
	}

	public void StopPathing()
	{
		pathing = false;
	}

	public Projectile CreateNoteWarning(Tile tile, Korevent evt)
	{
		if ((bool)tile)
		{
			Projectile projectile = spellObj.P.CreateCustomLerper(spellObj, tile, MovPattern.None, 0.1f, 0f, false, 100f, 0, "WarningNoteC");
			projectile.StartRhythmTimer(100f, false, evt, boss.playingKoreo, boss);
			projectile.tag = "Effect";
			return projectile;
		}
		return null;
	}

	public Projectile CreateWarning(Tile tile, Korevent evt)
	{
		if ((bool)tile)
		{
			Projectile projectile = spellObj.P.CreateCustomLerper(spellObj, tile, MovPattern.None, 0.1f, 0f, false, 100f, 0, "WarningDangerC");
			projectile.StartRhythmTimer(100f, true, evt, boss.playingKoreo, boss);
			projectile.tag = "Effect";
			return projectile;
		}
		return null;
	}

	public void CreateWarningPath()
	{
		edxTiles.Add(null);
		if (!pathing || boss.pendingEventIdw <= 0 || boss.pendingEventIdw >= edxTiles.Count)
		{
			return;
		}
		if (edxTiles[boss.pendingEventIdw - 1] == null)
		{
			if (boss.battleGrid.Get(new TileApp(Location.Player, Shape.Adjacent, Pattern.Moveable), 1, ctrl.currentPlayer).Count > 0)
			{
				edxTiles[boss.pendingEventIdw] = boss.battleGrid.Get(new TileApp(Location.Player, Shape.Adjacent, Pattern.Moveable), 1, ctrl.currentPlayer)[0];
			}
		}
		else if (boss.battleGrid.Get(new TileApp(Location.Tile, Shape.Adjacent, Pattern.Moveable, 1, edxTiles[boss.pendingEventIdw - 1]), 1, ctrl.currentPlayer).Count > 0)
		{
			edxTiles[boss.pendingEventIdw] = boss.battleGrid.Get(new TileApp(Location.Tile, Shape.Adjacent, Pattern.Moveable, 1, edxTiles[boss.pendingEventIdw - 1]), 1, ctrl.currentPlayer)[0];
		}
		if (edxTiles[boss.pendingEventIdw] != null)
		{
			notesPlayed++;
			CreateNoteWarning(edxTiles[boss.pendingEventIdw], boss.events[boss.pendingEventIdw]);
		}
		else
		{
			missedNotes++;
		}
	}

	public void CreateShotPath()
	{
		if (edxTiles.Count > boss.pendingEventIda && edxTiles[boss.pendingEventIda] != null)
		{
			KoreographyEvent koreographyEvent = boss.events[boss.pendingEventIda];
			Projectile projectile = spellObj.P.CreateCustomLerper(spellObj, edxTiles[boss.pendingEventIda], MovPattern.None, 0.1f, 0f, false, 0.2f, 0, animShot.name, castSound.name);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class Moveable : MonoBehaviour
{
	public float lerpTime = 0.1f;

	public float baseLerpTime = 0f;

	private float calculatedLerpTime = 0f;

	private float currentLerpTime;

	public List<float> lerpTimeMods = new List<float>();

	public Tile endTile;

	public Tile startTile;

	public Tile currentTile;

	public State state;

	public int alignNum = 0;

	public Being being;

	public Dictionary<int, bool> channelWatchers = new Dictionary<int, bool>();

	private int channelCount = 0;

	private bool queuedMove = false;

	public Direction direction = Direction.None;

	public Vector3 offset = Vector3.zero;

	public bool hovering = false;

	public bool offCurrentTile = false;

	public bool isProjectile = false;

	public bool forced = false;

	public MovPattern movement = MovPattern.None;

	public bool destroyIfNoTile = false;

	public bool unmoveable = false;

	public bool neverOccupy = false;

	public bool overrideMovement = true;

	public TI ti;

	public BattleGrid battleGrid;

	private State endState;

	private void Start()
	{
		startTile = currentTile;
		endTile = currentTile;
	}

	public void MoveTo(int tileX, int tileY, bool occupyStart = false, bool checkEndAlignment = true, bool hover = false, bool checkOccupied = true, bool endIdle = false, bool occupyEnd = true, bool force = false, bool triggerOnMove = true)
	{
		if (checkEndAlignment)
		{
			if (!offCurrentTile)
			{
				startTile = currentTile;
			}
		}
		else
		{
			startTile = endTile;
		}
		if (endIdle)
		{
			SetState(State.Idle);
		}
		if (MoveCheck(tileX, tileY, occupyStart, checkEndAlignment, hover, checkOccupied, occupyEnd, force))
		{
			if (force)
			{
				if (endIdle)
				{
					state = State.Idle;
				}
				forced = true;
			}
			if ((bool)being && being.mov == this && !being.dontMoveAnim && !being.dontInterruptAnim && !being.dontInterruptChannelAnim && !being.hitAnimationActive && (overrideMovement || !being.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Start")))
			{
				if (tileX > startTile.x)
				{
					if (FacingDirection() == 1)
					{
						being.anim.SetTrigger("front");
					}
					else
					{
						being.anim.SetTrigger("back");
					}
				}
				else if (tileX < startTile.x)
				{
					if (FacingDirection() == 1)
					{
						being.anim.SetTrigger("back");
					}
					else
					{
						being.anim.SetTrigger("front");
					}
				}
				else if (tileY > startTile.y)
				{
					being.anim.SetTrigger("hop");
				}
				else if (tileY < startTile.y)
				{
					being.anim.SetTrigger("drop");
				}
			}
			StartCoroutine(Moving(triggerOnMove));
		}
		else
		{
			endTile = startTile;
		}
	}

	public int FacingDirection()
	{
		return Mathf.RoundToInt(base.transform.TransformDirection(Vector3.right).x);
	}

	public void Move(int localAddX, int localAddY, bool occupyStart = false, bool checkEndAlignment = true, bool hover = false, bool checkOccupied = true, bool endIdle = false, bool occupyEnd = true, Being relativeBeing = null, bool force = false, bool triggerOnMove = true)
	{
		if (state == State.Moving && !queuedMove && !forced)
		{
			StartCoroutine(QueueMove(localAddX, localAddY, occupyStart, checkEndAlignment, hover, checkOccupied, endIdle, occupyEnd, relativeBeing, triggerOnMove));
			return;
		}
		if (relativeBeing == null)
		{
			relativeBeing = being;
		}
		if (relativeBeing != null)
		{
			localAddX *= Mathf.FloorToInt(relativeBeing.transform.right.x);
		}
		MoveTo(startTile.x + localAddX, startTile.y + localAddY, occupyStart, checkEndAlignment, hover, checkOccupied, endIdle, occupyEnd, force, triggerOnMove);
	}

	private IEnumerator QueueMove(int localAddX, int localAddY, bool occupyStart = false, bool checkEndAlignment = true, bool hover = false, bool checkOccupied = true, bool endIdle = false, bool occupyEnd = true, Being relativeBeing = null, bool force = false, bool triggerOnMove = true)
	{
		queuedMove = true;
		while (state == State.Moving)
		{
			yield return null;
		}
		queuedMove = false;
		Move(localAddX, localAddY, occupyStart, checkEndAlignment, hover, checkOccupied, endIdle, occupyEnd, relativeBeing, force, triggerOnMove);
	}

	public void MoveToTile(Tile tile, bool occupyStart = false, bool checkEndAlignment = true, bool hover = false, bool checkOccupied = true, bool endIdle = true, bool occupyEnd = true, bool force = false, bool triggerOnMove = true)
	{
		if ((bool)tile)
		{
			MoveTo(tile.x, tile.y, occupyStart, checkEndAlignment, hover, checkOccupied, endIdle, occupyEnd, force, triggerOnMove);
		}
	}

	public void MoveToTile(List<Tile> tileList, bool occupyStart = false, bool checkEndAlignment = true, bool hover = false, bool checkOccupied = true, bool endIdle = true, bool occupyEnd = true, bool force = false, bool triggerOnMove = true)
	{
		if (tileList.Count > 0)
		{
			MoveTo(tileList[0].x, tileList[0].y, occupyStart, checkEndAlignment, hover, checkOccupied, endIdle, occupyEnd, force, triggerOnMove);
		}
	}

	public void MoveToCurrentTile()
	{
		MoveTo(currentTile.x, currentTile.y, false, false);
	}

	public void TeleportTo(int tileX, int tileY, bool occupyStart = false, bool checkEndAlignment = true, bool hover = false, bool checkOccupied = true, bool endIdle = true, bool occupyEnd = true, bool force = false)
	{
		if (checkEndAlignment)
		{
			startTile = currentTile;
		}
		else
		{
			startTile = endTile;
		}
		if (MoveCheck(tileX, tileY, occupyStart, checkEndAlignment, hover, checkOccupied, occupyEnd, force))
		{
			startTile = endTile;
			base.transform.position = endTile.transform.position;
		}
		else
		{
			endTile = startTile;
		}
	}

	public void TeleportToTile(Tile tile, bool occupyStart = false, bool checkEndAlignment = true, bool hover = false, bool checkOccupied = true, bool endIdle = true, bool occupyEnd = true, bool force = false)
	{
		if ((bool)tile)
		{
			TeleportTo(tile.x, tile.y, occupyStart, checkEndAlignment, hover, checkOccupied, endIdle, occupyEnd, force);
		}
	}

	private IEnumerator Moving(bool triggerOnMove = true)
	{
		if (lerpTime > 0f)
		{
			BreakChannels();
			endState = state;
			if (endState == State.Channeling)
			{
				endState = State.Idle;
			}
			SetState(State.Moving);
			currentLerpTime = 0f;
			calculatedLerpTime = lerpTime;
			foreach (float ltMod in lerpTimeMods)
			{
				calculatedLerpTime += ltMod;
			}
			while (base.transform.position != endTile.transform.position + offset)
			{
				if ((bool)being && (bool)being.player && !isProjectile)
				{
					currentLerpTime += BC.playerChronoTime;
				}
				else
				{
					currentLerpTime += Time.deltaTime;
				}
				if (currentLerpTime > calculatedLerpTime)
				{
					currentLerpTime = calculatedLerpTime;
				}
				float t2 = currentLerpTime / calculatedLerpTime;
				t2 = Mathf.Sin(t2 * (float)Math.PI * 0.5f);
				base.transform.position = Vector3.Lerp(startTile.transform.position + offset, endTile.transform.position + offset, t2);
				yield return new WaitForEndOfFrame();
			}
			if ((bool)being && triggerOnMove)
			{
				if (!isProjectile)
				{
					being.TriggerArtifacts(FTrigger.OnMove);
				}
				if (being == being.player && !isProjectile)
				{
					being.TriggerAllArtifacts(FTrigger.OnPlayerMove);
					being.stepsTakenThisBattle++;
				}
			}
			SetState(endState);
		}
		else
		{
			base.transform.position = endTile.transform.position + offset;
		}
		if (forced)
		{
			forced = false;
		}
		startTile = endTile;
	}

	public bool MoveCheck(int x, int y, bool occupyStart = false, bool checkEndAlign = true, bool hover = false, bool checkOccupied = true, bool occupyEnd = true, bool force = false)
	{
		if (!battleGrid.TileExists(x, y) || ((bool)being && being.dead) || ((bool)being && (bool)GetComponent<Being>() && being.GetStatusEffect(Status.Root) != null && !force))
		{
			return false;
		}
		if (state == State.Moving && !force)
		{
			return false;
		}
		if (forced)
		{
			return false;
		}
		if (unmoveable)
		{
			return false;
		}
		endTile = battleGrid.grid[x, y];
		if (!battleGrid.grid[x, y].IsMoveable() && !hover && !hovering)
		{
			return false;
		}
		if (occupyStart && !checkEndAlign)
		{
			offCurrentTile = true;
			return true;
		}
		if (!occupyStart && !checkEndAlign)
		{
			currentTile = endTile;
			offCurrentTile = false;
			return true;
		}
		if (!occupyStart && checkEndAlign)
		{
			if (endTile.AlignedTo(alignNum))
			{
				if (checkOccupied && !endTile.vacant && endTile != currentTile)
				{
					return false;
				}
				if (offCurrentTile)
				{
					SetTileOccupation(currentTile, 0, hovering);
				}
				else if ((occupyStart || occupyEnd) && currentTile != endTile)
				{
					SetTileOccupation(startTile, 0, hovering);
				}
				currentTile = endTile;
				if (occupyEnd)
				{
					SetTileOccupation(endTile, 1, hovering);
				}
				offCurrentTile = false;
				return true;
			}
			if (offCurrentTile)
			{
				return true;
			}
		}
		return false;
	}

	public void SetTile(Tile tile)
	{
		startTile = tile;
		currentTile = tile;
		endTile = tile;
		SetTileOccupation(tile, 1, hovering);
	}

	public bool GetMoveableTile(int x, int y, bool returnBrokenTile = false)
	{
		if (battleGrid.TileExists(x, y))
		{
			Tile tile = battleGrid.grid[x, y];
			if (tile.AlignedTo(alignNum) && tile.vacant)
			{
				if (tile.type == TileType.None || (tile.type == TileType.Broken && !returnBrokenTile))
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private bool MoveTileCheck(int x, int y)
	{
		if (battleGrid.TileExists(x, y) && (battleGrid.grid[x, y].IsMoveable() || hovering))
		{
			return true;
		}
		return false;
	}

	public void SetState(string stateName)
	{
		SetState((State)Enum.Parse(typeof(State), stateName));
	}

	public void SetState(State newState)
	{
		BreakChannels();
		state = newState;
	}

	public void Move(MovPattern mv, int movements = -1, int current = -1)
	{
		switch (mv)
		{
		case MovPattern.PatrolRandomEmpty:
			PatrolRandomEmpty();
			break;
		case MovPattern.PatrolRandomEmptyHorizontal:
			PatrolRandomEmptyHorizontal();
			break;
		case MovPattern.PatrolRandomEmptyVertical:
			PatrolRandomEmptyVertical();
			break;
		case MovPattern.PatrolRandomEmptyVerticalToPlayer:
			PatrolRandomEmptyVerticalToPlayer();
			break;
		case MovPattern.DashRandomEmptyToPlayerRow:
			DashRandomEmptyToPlayerRow();
			break;
		case MovPattern.MoveToRandom:
			MoveToRandom();
			break;
		case MovPattern.TeleportRandom:
			TeleportRandom();
			break;
		case MovPattern.TeleportRandomEndFront:
			TeleportRandomEndFront(movements, current);
			break;
		case MovPattern.JumpToPlayerEndField:
			JumpToPlayerEndField(movements, current);
			break;
		case MovPattern.JumpRandomEndFront:
			JumpRandomEndFront(movements, current);
			break;
		case MovPattern.TeleportVerticalToPlayer:
			TeleportVerticalToPlayer();
			break;
		case MovPattern.PlayerHalfField:
			PlayerHalfField();
			break;
		case MovPattern.Clockwise:
			Clockwise();
			break;
		case MovPattern.CClockwise:
			CClockwise();
			break;
		case MovPattern.Forward:
			Forward();
			break;
		case MovPattern.Up:
			Up();
			break;
		case MovPattern.Down:
			Down();
			break;
		case MovPattern.Back:
			Back();
			break;
		case MovPattern.CClockwiseOtherEdges:
			CClockwiseOtherEdges();
			break;
		case MovPattern.BounceGrid:
			BounceGrid();
			break;
		case MovPattern.Diamond:
			Diamond();
			break;
		case MovPattern.ToPlayer:
			ToPlayer();
			break;
		case MovPattern.Ricochet:
			Ricochet();
			break;
		case MovPattern.ZigZag:
			ZigZag();
			break;
		}
	}

	public int CreateChannel()
	{
		SetState(State.Channeling);
		being.TriggerArtifacts(FTrigger.OnChannel);
		being.dontInterruptChannelAnim = true;
		channelCount++;
		channelWatchers[channelCount] = true;
		return channelCount;
	}

	public bool CheckChannel(int channelNum)
	{
		if (channelWatchers.ContainsKey(channelNum))
		{
			return channelWatchers[channelNum];
		}
		return false;
	}

	public void BreakChannels()
	{
		if (channelWatchers.Count > 0)
		{
			channelWatchers.Clear();
		}
		being.dontInterruptChannelAnim = false;
	}

	public void Forward()
	{
		if (battleGrid.TileExists(currentTile.x + 1, currentTile.y))
		{
			Move(1, 0, false, false, true);
		}
		else if (destroyIfNoTile)
		{
			SimplePool.Despawn(base.gameObject);
		}
	}

	public void Up()
	{
		if (battleGrid.TileExists(currentTile.x, currentTile.y + 1))
		{
			Move(0, 1, false, false, true);
		}
		else if (destroyIfNoTile)
		{
			SimplePool.Despawn(base.gameObject);
		}
	}

	public void Down()
	{
		if (battleGrid.TileExists(currentTile.x, currentTile.y - 1))
		{
			Move(0, -1, false, false, true);
		}
		else if (destroyIfNoTile)
		{
			SimplePool.Despawn(base.gameObject);
		}
	}

	public void Back()
	{
		if (battleGrid.TileExists(currentTile.x - 1, currentTile.y))
		{
			if (isProjectile)
			{
				Move(-1, 0, false, false, true);
			}
			else if (destroyIfNoTile)
			{
				SimplePool.Despawn(base.gameObject);
			}
			else
			{
				Move(-1, 0);
			}
		}
	}

	public void Reverse()
	{
		switch (movement)
		{
		case MovPattern.Forward:
			Back();
			break;
		case MovPattern.Back:
			Forward();
			break;
		case MovPattern.Up:
			Down();
			break;
		case MovPattern.Down:
			Up();
			break;
		}
	}

	public void PatrolRandom()
	{
		if (state == State.Idle)
		{
			switch (UnityEngine.Random.Range(0, 4))
			{
			case 0:
				Move(0, 1);
				break;
			case 1:
				Move(-1, 0);
				break;
			case 2:
				Move(0, -1);
				break;
			case 3:
				Move(1, 0);
				break;
			}
		}
	}

	public void PatrolRandomEmpty()
	{
		if (state == State.Idle)
		{
			List<Tile> list = battleGrid.Get(new TileApp(Location.Current, Shape.Adjacent, Pattern.Moveable), 1, being);
			int index = UnityEngine.Random.Range(0, list.Count);
			if (list.Count > 0)
			{
				MoveTo(list[index].x, list[index].y);
			}
		}
	}

	public void PatrolRandomEmptyHorizontal()
	{
		if (state == State.Idle)
		{
			List<Tile> list = new List<Tile>();
			if (GetMoveableTile(startTile.x - 1, startTile.y))
			{
				list.Add(battleGrid.grid[startTile.x - 1, startTile.y]);
			}
			if (GetMoveableTile(startTile.x + 1, startTile.y))
			{
				list.Add(battleGrid.grid[startTile.x + 1, startTile.y]);
			}
			int index = UnityEngine.Random.Range(0, list.Count);
			if (list.Count > 0)
			{
				MoveTo(list[index].x, list[index].y);
			}
		}
	}

	public void PatrolRandomEmptyVertical()
	{
		if (state == State.Idle)
		{
			List<Tile> list = new List<Tile>();
			if (GetMoveableTile(startTile.x, startTile.y + 1))
			{
				list.Add(battleGrid.grid[startTile.x, startTile.y + 1]);
			}
			if (GetMoveableTile(startTile.x, startTile.y - 1))
			{
				list.Add(battleGrid.grid[startTile.x, startTile.y - 1]);
			}
			int index = UnityEngine.Random.Range(0, list.Count);
			if (list.Count > 0)
			{
				MoveTo(list[index].x, list[index].y);
			}
		}
	}

	public void PatrolRandomEmptyVerticalToPlayer()
	{
		if (state != 0)
		{
			return;
		}
		Tile tile = null;
		if (being.ctrl.currentPlayer.mov.currentTile.y > currentTile.y)
		{
			if (GetMoveableTile(startTile.x, startTile.y + 1))
			{
				tile = battleGrid.grid[startTile.x, startTile.y + 1];
			}
		}
		else if (being.ctrl.currentPlayer.mov.currentTile.y < currentTile.y && GetMoveableTile(startTile.x, startTile.y - 1))
		{
			tile = battleGrid.grid[startTile.x, startTile.y - 1];
		}
		if (tile != null)
		{
			MoveTo(tile.x, tile.y);
		}
	}

	public void DashRandomEmptyToPlayerRow()
	{
		if (state == State.Idle)
		{
			MoveToTile(battleGrid.Get(new TileApp(Location.Player, Shape.Row, Pattern.Moveable), 0, being));
		}
	}

	private void CClockwiseOtherEdges()
	{
		int num = 0;
		int num2 = 0;
		switch (direction)
		{
		case Direction.Left:
			num = -1;
			break;
		case Direction.Down:
			num2 = -1;
			break;
		case Direction.Right:
			num = 1;
			break;
		case Direction.Up:
			num2 = 1;
			break;
		}
		int num3 = currentTile.x + num;
		int num4 = currentTile.y + num2;
		if (battleGrid.TileExists(num3, num4) && battleGrid.grid[num3, num4].align != alignNum && battleGrid.grid[num3, num4].IsMoveable())
		{
			MoveTo(num3, num4, false, false);
			return;
		}
		switch (direction)
		{
		case Direction.Left:
			direction = Direction.Down;
			break;
		case Direction.Down:
			direction = Direction.Right;
			break;
		case Direction.Right:
			direction = Direction.Up;
			break;
		case Direction.Up:
			direction = Direction.Left;
			break;
		}
	}

	private void ToPlayer()
	{
		SetTileOccupation(currentTile, 0, hovering);
		MoveTo(being.ctrl.currentPlayer.mov.endTile.x, being.ctrl.currentPlayer.mov.endTile.y, false, false, true, false, false, false);
	}

	private void JumpToPlayerEndField(int movements, int num)
	{
		if (state == State.Idle)
		{
			if (num == movements - 1)
			{
				MoveTo(being.mov.currentTile.x, being.mov.currentTile.y, false, false, true, false, true);
			}
			else
			{
				MoveTo(being.ctrl.currentPlayer.mov.endTile.x, being.ctrl.currentPlayer.mov.endTile.y, true, false, true, false, false, false);
			}
		}
	}

	private void SetTileOccupation(Tile tileToOccupy, int occupationNum, bool hovering = false)
	{
		if (!neverOccupy || occupationNum == 0)
		{
			tileToOccupy.SetOccupation(occupationNum, hovering);
		}
	}

	private void Ricochet()
	{
		int num = 0;
		int num2 = 0;
		switch (direction)
		{
		case Direction.DownLeft:
			num = -1;
			num2 = -1;
			break;
		case Direction.DownRight:
			num = 1;
			num2 = -1;
			break;
		case Direction.UpRight:
			num = 1;
			num2 = 1;
			break;
		case Direction.UpLeft:
			num2 = 1;
			num = -1;
			break;
		case Direction.DownForward:
			num = FacingDirection();
			num2 = -1;
			break;
		case Direction.UpForward:
			num = FacingDirection();
			num2 = 1;
			break;
		case Direction.DownBackward:
			num = -FacingDirection();
			num2 = -1;
			break;
		case Direction.UpBackward:
			num = -FacingDirection();
			num2 = 1;
			break;
		}
		int num3 = currentTile.x + num;
		int num4 = currentTile.y + num2;
		if (MoveTileCheck(num3, num4))
		{
			MoveTo(num3, num4, false, false);
			return;
		}
		if (!being.IsReference())
		{
			being.ctrl.camCtrl.Shake(0);
		}
		switch (direction)
		{
		case Direction.DownLeft:
			if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpLeft;
			}
			else if (MoveTileCheck(num3 + 2, num4))
			{
				direction = Direction.DownRight;
			}
			else
			{
				direction = Direction.UpRight;
			}
			break;
		case Direction.DownRight:
			if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpRight;
			}
			else if (MoveTileCheck(num3 - 2, num4))
			{
				direction = Direction.DownLeft;
			}
			else
			{
				direction = Direction.UpLeft;
			}
			break;
		case Direction.UpRight:
			if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownRight;
			}
			else if (MoveTileCheck(num3 - 2, num4))
			{
				direction = Direction.UpLeft;
			}
			else
			{
				direction = Direction.DownLeft;
			}
			break;
		case Direction.UpLeft:
			if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownLeft;
			}
			else if (MoveTileCheck(num3 + 2, num4))
			{
				direction = Direction.UpRight;
			}
			else
			{
				direction = Direction.DownRight;
			}
			break;
		case Direction.DownBackward:
			if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpBackward;
			}
			else if (MoveTileCheck(num3 + FacingDirection() * 2, num4))
			{
				direction = Direction.DownForward;
			}
			else
			{
				direction = Direction.UpForward;
			}
			break;
		case Direction.DownForward:
			if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpForward;
			}
			else if (MoveTileCheck(num3 - FacingDirection() * 2, num4))
			{
				direction = Direction.DownBackward;
			}
			else
			{
				direction = Direction.UpBackward;
			}
			break;
		case Direction.UpForward:
			if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownForward;
			}
			else if (MoveTileCheck(num3 - FacingDirection() * 2, num4))
			{
				direction = Direction.UpBackward;
			}
			else
			{
				direction = Direction.DownBackward;
			}
			break;
		case Direction.UpBackward:
			if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownBackward;
			}
			else if (MoveTileCheck(num3 + FacingDirection() * 2, num4))
			{
				direction = Direction.UpForward;
			}
			else
			{
				direction = Direction.DownForward;
			}
			break;
		case Direction.Forward:
		case Direction.Backward:
			break;
		}
	}

	private void BounceGrid()
	{
		int num = 0;
		int num2 = 0;
		switch (direction)
		{
		case Direction.Left:
			num = -1;
			break;
		case Direction.Down:
			num2 = -1;
			break;
		case Direction.Right:
			num = 1;
			break;
		case Direction.Up:
			num2 = 1;
			break;
		case Direction.Forward:
			num = FacingDirection();
			break;
		case Direction.Backward:
			num = -FacingDirection();
			break;
		}
		int num3 = currentTile.x + num;
		int num4 = currentTile.y + num2;
		if (MoveTileCheck(num3, num4))
		{
			MoveTo(num3, num4, false, false);
			return;
		}
		switch (direction)
		{
		case Direction.Left:
			direction = Direction.Right;
			break;
		case Direction.Down:
			num2 = -1;
			break;
		case Direction.Right:
			direction = Direction.Left;
			break;
		case Direction.Up:
			num2 = 1;
			break;
		case Direction.Backward:
			direction = Direction.Forward;
			break;
		case Direction.Forward:
			direction = Direction.Backward;
			break;
		}
	}

	private void Diamond()
	{
		int num = 0;
		int num2 = 0;
		switch (direction)
		{
		case Direction.DownLeft:
			num = -1;
			num2 = -1;
			break;
		case Direction.DownRight:
			num = 1;
			num2 = -1;
			break;
		case Direction.UpRight:
			num = 1;
			num2 = 1;
			break;
		case Direction.UpLeft:
			num2 = 1;
			num = -1;
			break;
		case Direction.DownBackward:
			num = -FacingDirection();
			num2 = -1;
			break;
		case Direction.DownForward:
			num = FacingDirection();
			num2 = -1;
			break;
		case Direction.UpForward:
			num = FacingDirection();
			num2 = 1;
			break;
		case Direction.UpBackward:
			num2 = 1;
			num = -FacingDirection();
			break;
		}
		int num3 = currentTile.x + num;
		int num4 = currentTile.y + num2;
		if (MoveTileCheck(num3, num4))
		{
			MoveTo(num3, num4, false, false);
			switch (direction)
			{
			case Direction.DownLeft:
				direction = Direction.DownRight;
				break;
			case Direction.DownRight:
				direction = Direction.UpRight;
				break;
			case Direction.UpRight:
				direction = Direction.UpLeft;
				break;
			case Direction.UpLeft:
				direction = Direction.DownLeft;
				break;
			case Direction.DownBackward:
				if (FacingDirection() == 1)
				{
					direction = Direction.DownForward;
				}
				else
				{
					direction = Direction.UpBackward;
				}
				break;
			case Direction.DownForward:
				if (FacingDirection() == 1)
				{
					direction = Direction.UpForward;
				}
				else
				{
					direction = Direction.DownBackward;
				}
				break;
			case Direction.UpForward:
				if (FacingDirection() == 1)
				{
					direction = Direction.UpBackward;
				}
				else
				{
					direction = Direction.DownForward;
				}
				break;
			case Direction.UpBackward:
				if (FacingDirection() == 1)
				{
					direction = Direction.DownBackward;
				}
				else
				{
					direction = Direction.UpForward;
				}
				break;
			case Direction.Forward:
			case Direction.Backward:
				break;
			}
			return;
		}
		if (!being.IsReference())
		{
			being.ctrl.camCtrl.Shake(0);
		}
		switch (direction)
		{
		case Direction.DownLeft:
			if (MoveTileCheck(num3 + 2, num4))
			{
				direction = Direction.DownRight;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpLeft;
			}
			break;
		case Direction.DownRight:
			if (MoveTileCheck(num3 - 2, num4))
			{
				direction = Direction.DownLeft;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpRight;
			}
			break;
		case Direction.UpRight:
			if (MoveTileCheck(num3 - 2, num4))
			{
				direction = Direction.UpLeft;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownRight;
			}
			break;
		case Direction.UpLeft:
			if (MoveTileCheck(num3 + 2, num4))
			{
				direction = Direction.UpRight;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownLeft;
			}
			break;
		case Direction.DownBackward:
			if (MoveTileCheck(num3 + FacingDirection() * 2, num4))
			{
				direction = Direction.DownForward;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpBackward;
			}
			break;
		case Direction.DownForward:
			if (MoveTileCheck(num3 - FacingDirection() * 2, num4))
			{
				direction = Direction.DownBackward;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpForward;
			}
			break;
		case Direction.UpForward:
			if (MoveTileCheck(num3 - FacingDirection() * 2, num4))
			{
				direction = Direction.UpBackward;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownForward;
			}
			break;
		case Direction.UpBackward:
			if (MoveTileCheck(num3 + FacingDirection() * 2, num4))
			{
				direction = Direction.UpForward;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownBackward;
			}
			break;
		case Direction.Forward:
		case Direction.Backward:
			break;
		}
	}

	private void ZigZag()
	{
		int num = 0;
		int num2 = 0;
		switch (direction)
		{
		case Direction.DownLeft:
			num = -1;
			num2 = -1;
			break;
		case Direction.DownRight:
			num = 1;
			num2 = -1;
			break;
		case Direction.UpRight:
			num = 1;
			num2 = 1;
			break;
		case Direction.UpLeft:
			num2 = 1;
			num = -1;
			break;
		case Direction.DownBackward:
			num = -FacingDirection();
			num2 = -1;
			break;
		case Direction.DownForward:
			num = FacingDirection();
			num2 = -1;
			break;
		case Direction.UpForward:
			num = FacingDirection();
			num2 = 1;
			break;
		case Direction.UpBackward:
			num2 = 1;
			num = -FacingDirection();
			break;
		}
		int num3 = currentTile.x + num;
		int num4 = currentTile.y + num2;
		if (MoveTileCheck(num3, num4))
		{
			MoveTo(num3, num4, false, false);
			switch (direction)
			{
			case Direction.DownLeft:
				direction = Direction.UpLeft;
				break;
			case Direction.DownRight:
				direction = Direction.UpRight;
				break;
			case Direction.UpRight:
				direction = Direction.DownRight;
				break;
			case Direction.UpLeft:
				direction = Direction.DownLeft;
				break;
			case Direction.DownBackward:
				direction = Direction.UpBackward;
				break;
			case Direction.DownForward:
				direction = Direction.UpForward;
				break;
			case Direction.UpForward:
				direction = Direction.DownForward;
				break;
			case Direction.UpBackward:
				direction = Direction.DownBackward;
				break;
			case Direction.Forward:
			case Direction.Backward:
				break;
			}
			return;
		}
		if (!being.IsReference())
		{
			being.ctrl.camCtrl.Shake(0);
		}
		switch (direction)
		{
		case Direction.DownLeft:
			if (MoveTileCheck(num3 + 2, num4))
			{
				direction = Direction.DownRight;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpLeft;
			}
			break;
		case Direction.DownRight:
			if (MoveTileCheck(num3 - 2, num4))
			{
				direction = Direction.DownLeft;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpRight;
			}
			break;
		case Direction.UpRight:
			if (MoveTileCheck(num3 - 2, num4))
			{
				direction = Direction.UpLeft;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownRight;
			}
			break;
		case Direction.UpLeft:
			if (MoveTileCheck(num3 + 2, num4))
			{
				direction = Direction.UpRight;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownLeft;
			}
			break;
		case Direction.DownBackward:
			if (MoveTileCheck(num3 + FacingDirection() * 2, num4))
			{
				direction = Direction.DownForward;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpBackward;
			}
			break;
		case Direction.DownForward:
			if (MoveTileCheck(num3 - FacingDirection() * 2, num4))
			{
				direction = Direction.DownBackward;
			}
			else if (MoveTileCheck(num3, num4 + 2))
			{
				direction = Direction.UpForward;
			}
			break;
		case Direction.UpForward:
			if (MoveTileCheck(num3 - FacingDirection() * 2, num4))
			{
				direction = Direction.UpBackward;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownForward;
			}
			break;
		case Direction.UpBackward:
			if (MoveTileCheck(num3 + FacingDirection() * 2, num4))
			{
				direction = Direction.UpForward;
			}
			else if (MoveTileCheck(num3, num4 - 2))
			{
				direction = Direction.DownBackward;
			}
			break;
		case Direction.Forward:
		case Direction.Backward:
			break;
		}
	}

	private void Return()
	{
		if (direction == Direction.Right)
		{
			if (currentTile.x == battleGrid.gridLength - 1)
			{
				direction = Direction.Left;
			}
		}
		else if (direction == Direction.Left && currentTile.x == 0)
		{
			direction = Direction.Right;
		}
		if (direction == Direction.Forward)
		{
			if (currentTile.x == battleGrid.gridLength - 1)
			{
				direction = Direction.Backward;
			}
		}
		else if (direction == Direction.Backward && currentTile.x == 0)
		{
			direction = Direction.Forward;
		}
	}

	public void JumpRandomEndFront(int movements, int num)
	{
		if (state != 0)
		{
			return;
		}
		if (num == movements - 1)
		{
			List<Tile> list = battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.Unoccupied, 3), 0, being);
			if (list.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				MoveTo(list[index].x, list[index].y);
			}
		}
		else
		{
			List<Tile> list2 = battleGrid.Get(new TileApp(Location.RandAlliedUnique, Shape.Default, Pattern.Unoccupied), 0, being);
			if (list2.Count > 0)
			{
				int index2 = UnityEngine.Random.Range(0, list2.Count);
				MoveTo(list2[index2].x, list2[index2].y);
			}
		}
	}

	public void TeleportRandom()
	{
		if (state == State.Idle)
		{
			List<Tile> list = battleGrid.Get(new TileApp(Location.RandAlliedUnique, Shape.Default, Pattern.Unoccupied), 0, being);
			list.Remove(currentTile);
			int index = UnityEngine.Random.Range(0, list.Count);
			TeleportTo(list[index].x, list[index].y, false, true, false, true, true, true, true);
		}
	}

	public void TeleportRandomEndFront(int movements, int num)
	{
		if (state != 0)
		{
			return;
		}
		if (num == movements - 1)
		{
			List<Tile> list = battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.Unoccupied, 3), 0, being);
			if (list.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				TeleportTo(list[index].x, list[index].y);
			}
		}
		else
		{
			List<Tile> list2 = battleGrid.Get(new TileApp(Location.RandAlliedUnique, Shape.Default, Pattern.Unoccupied), 0, being);
			if (list2.Count > 0)
			{
				int index2 = UnityEngine.Random.Range(0, list2.Count);
				TeleportTo(list2[index2].x, list2[index2].y);
			}
		}
	}

	public void TeleportVerticalToPlayer()
	{
		if (state != 0)
		{
			return;
		}
		Tile tile = null;
		if (GetMoveableTile(startTile.x, being.ctrl.currentPlayer.mov.currentTile.y))
		{
			MoveTo(startTile.x, being.ctrl.currentPlayer.mov.currentTile.y);
			return;
		}
		if (being.ctrl.currentPlayer.mov.currentTile.y > currentTile.y)
		{
			if (GetMoveableTile(startTile.x, startTile.y + 1))
			{
				tile = battleGrid.grid[startTile.x, startTile.y + 1];
			}
		}
		else if (being.ctrl.currentPlayer.mov.currentTile.y < currentTile.y && GetMoveableTile(startTile.x, startTile.y - 1))
		{
			tile = battleGrid.grid[startTile.x, startTile.y - 1];
		}
		if (tile != null)
		{
			MoveTo(tile.x, tile.y);
		}
	}

	public void PlayerHalfField()
	{
		if (state == State.Idle)
		{
			int num = battleGrid.gridLength / 2;
			if (being.ctrl.currentPlayer.mov.currentTile.y > 3)
			{
				num = -battleGrid.gridLength / 2;
			}
			MoveTowardsTile(battleGrid.grid[being.ctrl.currentPlayer.mov.currentTile.x + num, being.ctrl.currentPlayer.mov.currentTile.y]);
		}
	}

	public void MoveTowardsTile(Tile destinationTile)
	{
		if (destinationTile.y > startTile.y && GetMoveableTile(startTile.x, startTile.y + 1))
		{
			Move(0, 1);
		}
		else if (destinationTile.y < startTile.y && GetMoveableTile(startTile.x, startTile.y - 1))
		{
			Move(0, -1);
		}
		else if (destinationTile.x > startTile.x && GetMoveableTile(startTile.x - FacingDirection(), startTile.y))
		{
			Move(FacingDirection(), 0);
		}
		else if (destinationTile.x < startTile.x && GetMoveableTile(startTile.x + FacingDirection(), startTile.y))
		{
			Move(-1 * FacingDirection(), 0);
		}
	}

	public void MoveToRandom()
	{
		if (state == State.Idle)
		{
			List<Tile> list = battleGrid.Get(new TileApp(Location.RandAlliedUnique, Shape.Default, Pattern.Unoccupied), 0, being);
			if (list.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				MoveTo(list[index].x, list[index].y);
			}
		}
	}

	public void PatrolVertical()
	{
		if (state == State.Idle)
		{
			switch (UnityEngine.Random.Range(0, 2))
			{
			case 0:
				Move(0, 1);
				break;
			case 1:
				Move(0, -1);
				break;
			}
		}
	}

	private void Clockwise()
	{
		switch (direction)
		{
		case Direction.Left:
			Move(-1, 0);
			break;
		case Direction.Down:
			Move(0, -1);
			break;
		case Direction.Right:
			Move(1, 0);
			break;
		case Direction.Up:
			Move(0, 1);
			break;
		}
		switch (direction)
		{
		case Direction.Left:
			direction = Direction.Up;
			break;
		case Direction.Up:
			direction = Direction.Right;
			break;
		case Direction.Right:
			direction = Direction.Down;
			break;
		case Direction.Down:
			direction = Direction.Left;
			break;
		default:
			direction = Direction.Down;
			break;
		}
	}

	private void CClockwise()
	{
		switch (direction)
		{
		case Direction.Left:
			Move(-1, 0);
			break;
		case Direction.Down:
			Move(0, -1);
			break;
		case Direction.Right:
			Move(1, 0);
			break;
		case Direction.Up:
			Move(0, 1);
			break;
		}
		switch (direction)
		{
		case Direction.Left:
			direction = Direction.Down;
			break;
		case Direction.Down:
			direction = Direction.Right;
			break;
		case Direction.Right:
			direction = Direction.Up;
			break;
		case Direction.Up:
			direction = Direction.Left;
			break;
		default:
			direction = Direction.Up;
			break;
		}
	}
}

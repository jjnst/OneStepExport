-- EFFECTS ----------------------------------------------

function Anchor (item)
	item.anchor = true
end

function AddDefaultEnemyArt (item)
	item.ctrl.itemMan.AddDefaultEnemyArt(item.currentApp.value)
end

function AddToDeck (item)
	for x = 1, GetAmount(item) do
		for _, target in ipairs(GetTarget(item)) do
			if (item.currentApp.value == "SpellToCast") then
				target.player.duelDisk.AddLiveSpell(item, item.being.spellToCast.itemID, item.being, false, false)
			else
				target.player.duelDisk.AddLiveSpell(item, item.currentApp.value, item.being, false, false)
			end
		end
	end
end

function AddToDeckFront (item)
	for x = 1, GetAmount(item) do
		for _, target in ipairs(GetTarget(item)) do
			target.player.duelDisk.AddLiveSpell(item, item.currentApp.value, item.being, false, true)
		end
	end
end

function AddToDiscard (item)
	for x = 1, GetAmount(item) do
		for _, target in ipairs(GetTarget(item)) do
			target.player.duelDisk.AddLiveSpell(item, item.currentApp.value, item.being, true, false)
		end
	end
end

function AlterCard (item)
	if (item.HasParam("damage")) then
		item.deCtrl.itemMan.spellDictionary[item.currentApp.value].damage = tonumber(item.Param("damage"))
	end
	if (item.HasParam("mana")) then
		item.deCtrl.itemMan.spellDictionary[item.currentApp.value].mana = tonumber(item.Param("mana"))
	end
	if (item.HasParam("anchor")) then
		item.deCtrl.itemMan.spellDictionary[item.currentApp.value].anchor = item.Param("anchor") ~= "0"
		item.deCtrl.itemMan.spellDictionary[item.currentApp.value].RemoveEffect(Effect.Anchor)
	end
end

function AtkDmgBattle (item)

	if (item.being.battleGrid.InBattle()) then
		item.ctrl.PlaceStatusWithDuration(item, Status.AtkDmg)
	end
end

function Backfire (item)
	item.backfire = true
end

function BaseBossTier (item)
	item.ctrl.baseBossTier = GetRoundedAmount(item)
end

function Channel (item)
	item.channel = true
end

function Chrono (item)
	item.ctrl.PlaceStatusWithDuration(item, Status.Chrono)
	-- item.item.SetTimeScale(tonumber(GetAmount(item)), tonumber(item.currentApp.duration))
end

function Consume (item)
	item.consume = true
end

function CastSpell(item)
	-- local theBeing -- for reapers being able to blade throw
	-- if (item.currentApp.fTrigger == FTrigger.OnHit) then
	-- 	theBeing = ctrl.lastTargetHit
	-- else 
	-- 	theBeing = item.being
	-- end

	if (item.generatedSpell == nil or item.generatedSpell.spell == nil) then
		item.generatedSpell = nil
	end

	if (item.currentApp.value == "LastSpellCast") then
		local castNum = 1
		while (true) do
			if (item.being.spellsCastThisBattle.Count > castNum - 1) then
				if (item.being.spellsCastThisBattle[item.being.spellsCastThisBattle.Count-castNum].itemID == "Echo") then
					castNum = castNum + 1
				else 
					item.generatedSpell = item.deCtrl.CreateSpellBase(item.being.spellsCastThisBattle[item.being.spellsCastThisBattle.Count-castNum].Clone(), item.being, false, true)
					item.consume = item.being.spellsCastThisBattle[item.being.spellsCastThisBattle.Count-castNum].consume
					-- item.generatedSpell = item.deCtrl.CreateSpellBase(item.being.spellsCastThisBattle[item.being.spellsCastThisBattle.Count-castNum].itemID, item.being, false)
					break
				end
			else
				return
			end
		end
	elseif (item.currentApp.value == "ParentSpell") then
		if (item.parentSpell ~= nil) then
			item.generatedSpell = item.parentSpell
		end
	elseif (item.currentApp.value == "ThisSpell") then
		item.generatedSpell = item.deCtrl.CreateSpellBase(item, item.being, false)
	elseif (item.currentApp.value == "Random") then
		item.generatedSpell = item.deCtrl.CreateSpellBase(item.deCtrl.itemMan.GetRandomSpell().itemID, item.being, false)
	elseif (item.generatedSpell == nil or item.generatedSpell.itemID ~= item.currentApp.value) then
		item.generatedSpell = item.deCtrl.CreateSpellBase(item.currentApp.value, item.being, false)
	end

	if (item.type == ItemType.Spell) then
		if (item.originSpell == nil) then
			item.generatedSpell.originSpell = item
		else
			item.generatedSpell.originSpell = item.originSpell
		end

		item.generatedSpell.parentSpell = item
	end
	
	item.generatedSpell.interrupt = false
	item.generatedSpell.StartCast(false, 0, false)
end

function CastVisual(item)
	if (item.generatedSpell == nil or item.generatedSpell.itemID ~= item.currentApp.value) then
		item.generatedSpell = item.deCtrl.CreateSpellBase(item.currentApp.value, item.being, false)
	end
		
	if (item.type == ItemType.Spell) then
		item.generatedSpell.parentSpell = item
	end

	item.generatedSpell.interrupt = false
	item.generatedSpell.StartCast(false, 0, false)
end

function Damage(item) --Pierces defense
	for _, target in ipairs(GetTarget(item)) do
		target.Damage (GetAmount(item), true)
	end
end

function DamageTrue(item)
	for _, target in ipairs(GetTarget(item)) do
		target.Damage (GetAmount(item), true, true)
	end
end

function DamageStatus (item)
	local statusEffect = item.hitBeing.GetStatusEffect(item.currentApp.value)
	if (statusEffect ~= nil) then
		item.hitBeing.health.ModifyHealth(Mathf.RoundToInt(-statusEffect.amount * GetAmount(item)))
	end
end

function DefenseBattle (item)
	if (item.being.battleGrid.InBattle()) then
		item.ctrl.PlaceStatusWithDuration(item, Status.Defense)
	end
end

function Devour (item)
	for _, target in ipairs(GetTarget(item)) do
		target.player.duelDisk.LaunchSlot(item.currentApp.amount, true)
	end
end

function DisableManualShuffle (item)

	for _, target in ipairs(GetTarget(item)) do
		while (target.player.duelDisk == nil) do
			WaitForSeconds(item, 0)
		end
		target.player.DisableManualShuffle(item)
	end
end

function DoubleCast (item)
	WaitForSeconds(item, 0.2)
	WaitUntil(item.being, State.Idle)
	-- if (item.being.battleGrid.InBattle()) then 
	-- print("doublecasting")
	item.StartCast(true, 0, true)
	-- end
end

function Eject (item)
	for _, target in ipairs(GetTarget(item)) do
		target.player.duelDisk.LaunchSlot(item.currentApp.amount, false)
	end
end

function EvilHostages (item)
	item.ctrl.ActivateEvilHostages()
end

function FlowStack (item)
	item.being.AddStatus(Status.Flow, GetAmount(item))
end

function Flame (item)
	if (item.currentApp.fTrigger == FTrigger.OnHit) then
		theTile = item.hitTile
	elseif (item.currentApp.fTrigger == FTrigger.TouchTile) then
		theTile = item.touchedTile
	end

	theTile.Flame(item)
end

function Fragile (item)
	item.ctrl.PlaceStatusNoDuration(item, Status.Fragile)
end

function FragileChange (item)
	BC.fragileMultiplier = GetAmount(item)
end

function Frost (item)
	item.ctrl.PlaceStatusNoDuration(item, Status.Frost)
end

function Haste (item)
	item.ctrl.PlaceStatusWithDuration(item, Status.Haste)
end

function Heal (item)
	for _, target in ipairs(GetTarget(item)) do
		target.Heal (GetRoundedAmount(item))
	end
end

function HealBattle (item)
	if (item.being.battleGrid.InBattle()) then
		for _, target in ipairs(GetTarget(item)) do
			target.Heal (GetRoundedAmount(item))
		end
	else
		for _, target in ipairs(GetTarget(item)) do
			target.MustBeInBattleWarning()
		end
	end
end

function Hit (item)
	for _, target in ipairs(GetTarget(item)) do
		target.HitAmount(GetRoundedAmount(item), true)
	end
end

function HitPierceDefense (item)
	for _, target in ipairs(GetTarget(item)) do
		target.HitAmount(GetRoundedAmount(item), true, true, nil, true, false)
	end
end

function HitTrue (item)
	for _, target in ipairs(GetTarget(item)) do
		target.HitAmount(GetRoundedAmount(item), true, true, nil, true, true)
	end
end

function Invincible (item)
	if (item.being.battleGrid.InBattle()) then
		for _, target in ipairs(GetTarget(item)) do
			target.AddCappedInvince(GetDuration(item))
		end
	else
		for _, target in ipairs(GetTarget(item)) do
			target.MustBeInBattleWarning()
		end
	end
end

function Jam (item)
	for x = 1, GetAmount(item) do
		for _, target in ipairs(GetTarget(item)) do
			if (target.player ~= nil) then
				target.player.duelDisk.AddLiveSpell(item, "Jam", target.player, false, false)
			end
		end
	end
end

function Link (item)
	item.ctrl.PlaceStatusWithDuration(item, Status.Link)
end

function Mana (item)
	for _, target in ipairs(GetTarget(item)) do
		if (target.player) then
			target.player.duelDisk.currentMana = target.player.duelDisk.currentMana + GetAmount(item)
		end
	end
end

function ManaRegenBattle (item)
	item.ctrl.PlaceStatusWithDuration(item, Status.ManaRegen)
end

function MaxManaBattle (item)
	if (item.being.battleGrid.InBattle()) then
		item.ctrl.PlaceStatusWithDuration(item, Status.MaxMana)
	else
		if (GetDuration(item) > 1) then
			for _, target in ipairs(GetTarget(item)) do
				target.MustBeInBattleWarning()
			end
		end
	end
end

function Money(item)
	if item.being.battleGrid == item.ctrl.ti.mainBattleGrid then
		item.ctrl.shopCtrl.ModifySera(GetRoundedAmount(item))
	end
end

function MoneyBattle(item)
	if item.being.battleGrid == item.ctrl.ti.mainBattleGrid then
		if (item.being.battleGrid.InBattle()) then
			item.ctrl.shopCtrl.ModifySera(GetRoundedAmount(item))
		end
	end
end

function Move (item)
	for _, target in ipairs(GetTarget(item)) do
		local totalX = GetRoundedAmount(item) --Uses "Amount as x value"
		local totalY = GetRoundedDuration(item) -- Uses "Duration as y value"
		local largerNum = math.abs(totalY)
		local currentApp = item.currentApp

		if (math.abs(totalX) > math.abs(totalY)) then
			largerNum = math.abs(totalX)
		end

		for x = 0, largerNum-1 do
			local tempX = 0
			local tempY = 0

			if (totalX > 0) then
				tempX = 1
				totalX = totalX - 1
			elseif (totalX < 0) then
				tempX = -1
				totalX = totalX + 1
			end

			if (totalY > 0) then
				tempY = 1
				totalY = totalY - 1
			elseif (totalY < 0) then
				tempY = -1
				totalY = totalY + 1
			end

			WaitWhileBeingState(target, State.Moving)

			if (currentApp.value == "ignoreOccupy") then
				target.mov.currentTile.SetOccupation(0, target.mov.hovering)
				target.mov.Move (tempX, tempY, false, false, true, false, false, false, item.being)
			else
				target.mov.Move (tempX, tempY, false, true, false, true, true, true, item.being)
			end


			WaitForSeconds(item, 0)
		end
	end
end

function NextShuffleTime (item)
	for _, target in ipairs(GetTarget(item)) do
		target.player.duelDisk.nextShuffleTimeModifier = target.player.duelDisk.nextShuffleTimeModifier + GetAmount(item)
	end
end

function Pet (item)

	for _, target in ipairs(GetTarget(item)) do
		local tiles = item.Get(item.currentApp.tileApp, 0)

		if (tiles.Count > 0) then

			item.item.SummonAfter(item, tiles[0], 0, item.currentApp, true)

		end
	end
end

function PlaySound (item)
	item.being.PlayOnce(item.currentApp.value)
end

function Poison (item)
	item.ctrl.PlaceStatusNoDuration(item, Status.Poison)
end

function PoisonMultiplyDuration (item)
	BC.poisonDuration = BC.poisonDuration * GetAmount(item)
end

function PoisonSetMinimum (item)
	BC.poisonMinimum = GetAmount(item)
end

function PowerUp (item)
	if (item.being.battleGrid.InBattle()) then
		item.tempDamage = item.tempDamage + GetAmount(item)
	end
end

function PowerUpPerm (item)
	if (item.being.IsReference() == false) then
		item.permDamage = item.permDamage + GetAmount(item)
	end
end

function RemoveStatus (item)
	for _, target in ipairs(GetTarget(item)) do
		target.RemoveStatus(item.currentApp.value)
	end
end

function RemoveStatusFromItem (item)
	for _, target in ipairs(GetTarget(item)) do
		target.RemoveStatusFromItem(item, item.currentApp.value)
	end
end

function Redeck (item)
	for _, target in ipairs(GetTarget(item)) do
		target.player.duelDisk.QueueCardtridge(item.cardtridge)
	end
end

function Reflect (item)
	item.ctrl.PlaceStatusWithDuration(item, Status.Reflect)
end

function RemoveBuff (item)
	for _, target in ipairs(GetTarget(item)) do
		target.RemoveBuff()
	end
end

function Root (item)
	item.ctrl.PlaceStatusWithDuration(item, Status.Root)
end

function MaxHPChange(item)
	item.being.player.health.SetMax(item.being.player.health.max + GetRoundedAmount(item))
	item.deCtrl.statsScreen.UpdateStats()
end

function MaxHPChangeBattle(item)
	for _, target in ipairs(GetTarget(item)) do
		-- target.health.max = target.health.max + GetAmount(item)
		if (item.being.battleGrid.InBattle()) then
			target.health.SetHealth(target.health.current, target.health.max + GetRoundedAmount(item))
		else
			target.MustBeInBattleWarning()
		end
	end
end

function MaxHPSet(item)
	for _, target in ipairs(GetTarget(item)) do
		-- target.health.max = target.health.max + GetAmount(item)
		target.health.SetMax(GetRoundedAmount(item))
	end
	item.deCtrl.statsScreen.UpdateStats()
	-- item.ctrl.currentPlayer.health.ModifyHealth (GetAmount(item))
end

function ParentOnHit(item)
	if (item.parentSpell ~= nil) then
		for _, target in ipairs(GetTarget(item)) do
			item.parentSpell.OnHit(target)
		end
	end
end

function PierceShield(item)
	-- print(item.currentApp.value)
	if (item.currentApp.value == "false") then
		item.spellObj.pierceShield = false
	else
		item.spellObj.pierceShield = true
	end
end

function Slow (item)
	item.ctrl.PlaceStatusWithDuration(item, Status.Slow)
end

function Shield (item)
	for _, target in ipairs(GetTarget(item)) do
		if (item.being.battleGrid.InBattle()) then
			target.AddStatus(Status.Shield, GetRoundedAmount(item))
		else
			if (GetRoundedAmount(item) > 5) then
				target.MustBeInBattleWarning()
			end
		end
	end

	-- Else display no shield out of battle message?
end

function ShieldDecayChange (item)
	BC.shieldDecay = BC.shieldDecay + GetAmount(item)
end

function ShieldDefense (item)
	for _, target in ipairs(GetTarget(item)) do
		target.shieldDefense = true
	end
end

function ShieldExte (item)
	if (item.being.battleGrid.InBattle()) then
		for _, target in ipairs(GetTarget(item)) do
			target.AddStatus(Status.ShieldExte, GetRoundedAmount(item))
		end
	else
		for _, target in ipairs(GetTarget(item)) do
			target.MustBeInBattleWarning()
		end
	end

	-- Else display no shield out of battle message?
end

function ShieldSet (item)
	
	if (item.being.battleGrid.InBattle()) then
		for _, target in ipairs(GetTarget(item)) do
			if (target.HasStatusEffect(Status.Shield) == false and GetAmount(item) > 0) then
				target.AddStatus(Status.Shield, GetRoundedAmount(item))
			else
				target.health.SetShield(GetRoundedAmount(item))
			end
		end
	else
		for _, target in ipairs(GetTarget(item)) do
			target.MustBeInBattleWarning()
		end
	end
end

function Shuffle (item)
	for _, target in ipairs(GetTarget(item)) do
		target.player.duelDisk.ManualShuffle()
	end
end

function SpellPowerBattle (item)
	if (item.being.battleGrid.InBattle()) then
		-- item.being.AddStatus(Status.SpellPower, GetAmount(item))
		item.ctrl.PlaceStatusWithDuration(item, Status.SpellPower)
	end
end

function Summon (item)

	-- Intro(item)
	local tiles = item.Get(item.numTiles)
	-- local touchedTile = item.touchedTile

	if (item.currentApp.fTrigger == FTrigger.TouchTile) then
		SummonAfter(item, item.touchedTile, item.shotDuration, item.currentApp)
		Outro(item)
		return
	else 
		
		for _, tile in ipairs(tiles) do
			if (item.damage ~= nil) then
				if (item.being.IsEnemy()) then
					item.P.CreateWarning (item, tile, item.warningDuration)
					SummonAfter(item, tile, item.warningDuration, item.currentApp)
				else
					item.P.CreateShot (item, tile)
					item.P.CreateBlastEffect (item, tile)
					SummonAfter(item, tile, item.blastDuration, item.currentApp)
				end
			end

		end
	end
	
	Outro(item)
end

function Teleport (item)
	for _, target in ipairs(GetTarget(item)) do
		local tiles = item.Get(item.currentApp.tileApp, 0)

		if (tiles.Count > 0) then
			target.mov.TeleportToTile(tiles[0], false, true, false, true, true, true, true);

			if (item.generatedSpell == nil or item.generatedSpell.itemID ~= item.currentApp.value) then
				item.generatedSpell = item.deCtrl.CreateSpellBase("TeleportVisual", item.being, false)
			end
		
			if (item.type == ItemType.Spell) then
				item.generatedSpell.parentSpell = item
			end

			item.being.savedTileList = tiles
			
			item.generatedSpell.interrupt = false
			item.generatedSpell.StartCast(false, 0, false)
		end
	end
end

function TeleportTo (item) 
	for _, target in ipairs(GetTarget(item)) do
		local totalX = GetRoundedAmount(item)
		local totalY = GetRoundedDuration(item)
		local largerNum = math.abs(totalY)

		if (math.abs(totalX) > math.abs(totalY)) then
			largerNum = math.abs(totalX)
		end

		for x = 0, largerNum-1 do
			local tempX = 0
			local tempY = 0

			if (totalX > 0) then
				tempX = 1
				totalX = totalX - 1
			elseif (totalX < 0) then
				tempX = -1
				totalX = totalX + 1
			end

			if (totalY > 0) then
				tempY = 1
				totalY = totalY - 1
			elseif (totalY < 0) then
				tempY = -1
				totalY = totalY + 1
			end
			WaitWhileBeingState(target, State.Moving)

			target.mov.TeleportTo (tempX, tempY, false, true, false, true, true, true, item.being)

			WaitForSeconds(item, 0)
		end
	end
end

function TileBreak (item)
	item.touchedTile.Break(GetDuration(item))
end

function TileCrack (item)
	item.touchedTile.Crack(GetDuration(item))
end

function TileFix (item)
	item.touchedTile.Fix()
end

function TriggerShuffleArtifacts (item)
	for _, target in ipairs(GetTarget(item)) do
		target.player.TriggerAllArtifacts(FTrigger.OnReshuffle)
	end
end

function Trinity (item)
	for _, target in ipairs(GetTarget(item)) do
		target.AddStatus(Status.Trinity, GetAmount(item))
	end
end
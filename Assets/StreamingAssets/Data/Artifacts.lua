-- function DamageEnemies(item)
-- 	for i = item.ctrl.currentEnemies.Count-1, 0, -1 do
-- 		item.ctrl.GetEnemy(i).Damage (GetAmount(item))
-- 	end
-- end

-- function DamageEnemies(item)
-- 	for i = item.ctrl.currentEnemies.Count-1, 0, -1 do
-- 		item.ctrl.GetEnemy(i).Damage (GetAmount(item))
-- 	end
-- end

-- function DamageRandomEnemy(item)
-- 	for i = item.ctrl.currentEnemies.Count-1, 0, -1 do
-- 		item.ctrl.GetEnemy(math.random(0, item.ctrl.currentEnemies.Count-1)).Damage (GetAmount(item))
-- 	end
-- end

function HoverTile(item)
	item.being.mov.hovering = true
end

function EquipWep(item)
    item.deCtrl.EquipWep(item.currentApp.value, item.being.player)
end

function SetFlow(item)
	item.being.theSpellCast.flow = true
end

function TriggerFlow(item)
	WaitForSeconds(item, 0.2)
	item.being.theSpellCast.Trigger(FTrigger.Flow)
end

function Deplete(item)
	item.Deplete();
end

function Replete(item)
	item.Replete();
end

function AtkDmg (item)
	item.atkDmg = ModifyStat(item, item.atkDmg)
end

function Defense (item)
	item.defense = ModifyStat(item, item.defense)
end

function Luck (item)
	item.ctrl.poCtrl.luck = ModifyStat(item, item.ctrl.poCtrl.luck)
end

function LuckPermanent (item)
	item.ctrl.poCtrl.permanentLuck = ModifyStat(item, item.ctrl.poCtrl.permanentLuck)
end

function MaxMana (item)
	item.maxMana = ModifyStat(item, item.maxMana)
end

function ManaRegen (item)
	item.manaRegen = ModifyStat(item, item.manaRegen)
end

function SpellPower (item)
	item.spellPower = ModifyStat(item, item.spellPower)
end

function ShuffleTime (item)
	item.shuffleTime = ModifyStat(item, item.shuffleTime)
end

function Removal (item)
	item.ctrl.shopCtrl.ModifyRemovalCount(GetAmount(item))
end

function Upgrader (item)
	item.ctrl.shopCtrl.ModifyUpgraderCount(GetAmount(item))
end

function Wrap (item)
	item.being.player.AddWrap(item)
end

function ModifyStat(item, current)
	if (item.currentApp.value == "Set") then
		return GetAmount(item)
	else
		return current + GetAmount(item)
	end
end
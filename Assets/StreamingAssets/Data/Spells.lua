function Multishot (spell)

	local channelNum = StartChannel(spell)
	local storedBeing = spell.being

	if (spell.introSound) then
		spell.being.PlayOnce(spell.introSound)
	end
	
	if (spell.fireLoop == FireLoop.While) then
		spell.beingAnim.SetBool("fireWhile", true)
	end
	
	if (spell.fireAnim == true and spell.fireAnimLate == false) then
		if (spell.fireLoop == FireLoop.Normal) then
			spell.beingAnim.SetTrigger("fire")
		end
	end

	if (spell.introDelay > 0) then
		WaitForSeconds(spell, spell.introDelay)
	end
	
	spell.tiles = spell.Get(spell.numTiles)
	
	local finalX = 0
	local numShots = spell.numShots
	if numShots < 0 then
		numShots = spell.tiles.Count
	end

	for x = 0, numShots-1 do
		if (spell.channel and spell.being.mov.CheckChannel(channelNum) == false) then
			break
		end

		if (spell.tiles.Count <= finalX or spell.tiles.Count == 1) then
			finalX = 0
		end

		if (spell.tiles.Count == 1) then
			spell.tiles = spell.Get(spell.numTiles)
		elseif (spell.tiles.Count < 1) then
			break;
		end

		if(spell.being.IsEnemy() or spell.warningDuration > 0) then
			spell.P.CreateWarning (spell, spell.tiles[finalX], spell.warningDuration)
			-- WaitForSeconds(spell, spell.warningDuration)
		end

		spell.P.CreateCastEffect (spell, spell.tiles[finalX], false, 0, x, spell.timeBetweenShots, true)
		
		
		if (spell.HasParam("lerpPattern")) then
			spell.spell.CreateLerperAfter(spell, spell.tiles[finalX], spell.castDelay, spell.gunPointSetting)
		else
			spell.spell.CreateShotAfter(spell, spell.tiles[finalX], spell.castDelay+spell.shotDelay, spell.gunPointSetting, x, spell.timeBetweenShots)
		end
		spell.spell.CreateBlastEffectAfter(spell, spell.tiles[finalX], spell.castDelay, spell.gunPointSetting)
		if (spell.timeBetweenShots > 0) then
			WaitForSeconds(spell, spell.timeBetweenShots)
		end
		finalX = finalX + 1
	end

	WaitForSeconds(spell, spell.castDelay)
	
	Outro(spell, storedBeing)
end

function StepSlash (spell)
	if (spell.introSound) then
		spell.being.PlayOnce(spell.introSound)
	end

	spell.tiles = spell.Get(spell.numTiles)
	WaitForSeconds(spell, spell.castDuration)

	for i = 0, spell.tiles.Count-1 do
	
		spell.being.mov.SetState(State.Attacking)
		
		spell.beingAnim.SetBool("dashing", true)
		-- local int xDash = spell.dashDistance
		-- local int yDash = spell.dashHeight
		-- if (xDash ~= 0 or yDash ~= 0) then
		spell.being.mov.MoveTo (spell.tiles[i].x, spell.tiles[i].y, true, false, true, false, false, false)
		-- spell.being.mov.MoveToTile (spell.being.TileLocal(xDash, yDash), true, false, true, false, false)
		WaitWhileMoving(spell)
		-- end
		spell.being.mov.SetState(State.Attacking)
		
		spell.beingAnim.SetBool("dashing", false)

		if (spell.fireAnim == true and spell.being.player ~= nil) then
			spell.beingAnim.SetTrigger ("spellCast") --For cpu
		end

		if (spell.fireAnim == true and spell.being.player ~= nil) then
			spell.beingAnim.ResetTrigger ("toIdle") --For cpu
		end

		-- local newEffect = spell.P.CreateBlastEffect (spell, spell.slashLocation, 0)
		
		-- newEffect.SetToGunPoint()
		
		if (spell.anchor == false) then
			spell.being.mov.SetState(State.Idle)
		end

		if (spell.castDelay > 0) then
			WaitForSeconds(spell, spell.castDelay)
		end

		if (spell.fireAnim == true) then
			spell.beingAnim.SetTrigger ("fire") --For cpu
		end
		
		
		for x = 0, spell.numShots-1 do
			local newAtk = spell.P.CreateShot (spell, spell.slashLocation, 0, true)
			
			newAtk.SetToGunPoint()

			spell.P.CreateBlastEffect(spell, spell.slashLocation, 0, true, spell.shotVelocity, true)
			
			if (spell.timeBetweenShots > 0) then
				WaitForSeconds(spell, spell.timeBetweenShots)
			end
		end
	end
	
	WaitForSeconds(spell, spell.recoveryTime)
	
	spell.being.mov.MoveTo (spell.being.mov.currentTile.x, spell.being.mov.currentTile.y, false, false, false, false, true, true, true)
	spell.beingAnim.SetTrigger("back")
	WaitWhileMoving(spell)
	
	Outro(spell)
end

function Mortar (spell)
	spell.alwaysUseGameTime = true
	local channelNum = StartChannel(spell)
	Intro(spell)

	for x = 0, spell.numShots-1 do
	
		local targetTiles = spell.Get(spell.numTiles)
		if (targetTiles.Count < 1) then
			break
		end
	
		if (spell.fireAnim == true) then
			if (spell.fireLoop == FireLoop.Normal) then
				spell.beingAnim.SetTrigger("fire")
			end
		end

		local tileCount = targetTiles.Count
		local altTileCount = 0

		if (spell.tileApps.Count > 1) then
			altTileCount = spell.Get(spell.numTiles, 1).Count
			if (tileCount < altTileCount or spell.HasParam("useAltTileCount")) then
				tileCount = altTileCount
			end
		end

		for i = 0, tileCount-1 do
			if (spell.channel and spell.being.mov.CheckChannel(channelNum) == false) then
				break
			end

			local shotNum = 0
			local targetTile = targetTiles[0]
			if (targetTiles.Count <= i) then
				targetTile = targetTiles[targetTiles.Count-1]
			else 
				targetTile = targetTiles[i]
			end
			if(spell.being.IsEnemy() or spell.warningDuration > 0 or spell.ctrl.pvpMode) then
				spell.P.CreateWarning (spell, targetTile, spell.timeToTravel)
			end

			if (targetTile == nil) then
				break
			end
			
			spell.P.CreateCastEffect (spell, spell.being.mov.currentTile, false, 0, i, spell.shotVelocity, true)
			
			local mortarFlier = spell.P.CreateAnimObj (spell, targetTile, spell.timeToTravel, false, 0, i, spell.shotVelocity)
			
			if (mortarFlier) then 
				local mortarFlierStartPos = spell.being.gunPoint.transform.position

				if (spell.tileApps.Count > 1) then
					if (altTileCount <= i) then
						mortarFlierStartPos = spell.Get(spell.numTiles, 1)[altTileCount-1].transform.position
						spell.P.CreateCastEffect (spell, spell.Get(spell.numTiles, 1)[altTileCount-1], false, 0, i, spell.shotVelocity)
					else 
						mortarFlierStartPos = spell.Get(spell.numTiles, 1)[i].transform.position
						spell.P.CreateCastEffect (spell, spell.Get(spell.numTiles, 1)[i], false, 0, i, spell.shotVelocity)
					end
					

					if mortarFlierStartPos == nil then
						mortarFlierStartPos = spell.being.gunPoint.transform.position
					end
				end
				mortarFlier.Arc(mortarFlierStartPos, targetTile.transform.position, spell.bending, spell.timeToTravel)
			end
			
			spell.spell.CreateBlastEffectAfter(spell, targetTile, spell.timeToTravel, GunPointSetting.None)
			spell.spell.CreateShotAfter(spell, targetTile, spell.timeToTravel, GunPointSetting.None, i, spell.shotVelocity)
			
			if (spell.shotVelocity > 0) then
				WaitForSeconds(spell, spell.shotVelocity)
			end
			shotNum = shotNum + 1
		end
		
		WaitForSeconds(spell, spell.timeBetweenShots)
	end
	
	Outro(spell)
end

function Storm (spell)
	local channelNum = StartChannel(spell)
	Intro(spell)
	spell.P.CreateCastEffect (spell, spell.being.mov.currentTile, false, 0, 0, 1, true)
	local numOfWaves = 1
	local timeBetweenWaves = 0
	if (spell.HasParam("numOfWaves")) then
		numOfWaves = tonumber(spell.Param("numOfWaves"))
	end
	if (spell.HasParam("timeBetweenWaves")) then
		timeBetweenWaves = tonumber(spell.Param("timeBetweenWaves"))
	end

	if (spell.fireAnim == true) then
		spell.beingAnim.SetTrigger ("fire")
	end
	
	for x = 0, numOfWaves-1 do
		local tiles = spell.Get(spell.numShots)
		
		for i = 0, tiles.Count-1 do
			if (spell.channel and spell.being.mov.CheckChannel(channelNum) == false) then
				break
			end
			local tile = tiles[i]
			if (spell.being.IsEnemy() or spell.warningDuration > 0 or spell.ctrl.pvpMode) then
				spell.P.CreateWarning (spell, tile, spell.warningDuration + spell.shotDelay)
			end

			spell.spell.CreateBlastEffectAfter(spell, tile, spell.warningDuration, spell.gunPointSetting)
			spell.spell.CreateShotAfter(spell, tile, spell.warningDuration + spell.shotDelay, spell.gunPointSetting, i, spell.timeBetweenShots)
			if (spell.timeBetweenShots > 0) then
				WaitForSeconds(spell, spell.timeBetweenShots)
			end
		end
		
		WaitForSeconds(spell, timeBetweenWaves)
	end
	spell.beingAnim.SetTrigger ("toIdle")
	
	Outro(spell)
end

function MagicClaw (spell)
	Intro(spell)
	local tiles = spell.Get(spell.numTiles)
	
	spell.P.CreateBlastEffect(spell, tiles[0])
	
	if (spell.being.FacingDirection() == 1) then
		local topRight = spell.P.CreateCustomShot(spell, tiles[0].x + 1, tiles[0].y + 1, false, spell.shotDuration, spell.damage)
		topRight.SetVelocity(spell.shotVelocity, Direction.DownLeft)
	else
		local topRight = spell.P.CreateCustomShot(spell, tiles[0].x - 1, tiles[0].y + 1, false, spell.shotDuration, spell.damage)
		topRight.SetVelocity(spell.shotVelocity, Direction.DownLeft)
	end
	
	WaitForSeconds(spell, 0.16)
	
	if (spell.being.FacingDirection() == 1) then
		local topLeft = spell.P.CreateCustomShot(spell, tiles[0].x - 1, tiles[0].y + 1, false, spell.shotDuration, spell.damage)
		topLeft.SetVelocity(spell.shotVelocity, Direction.DownRight)
	else
		local topLeft = spell.P.CreateCustomShot(spell, tiles[0].x + 1, tiles[0].y + 1, false, spell.shotDuration, spell.damage)
		topLeft.SetVelocity(spell.shotVelocity, Direction.DownRight)
	end
	
	Outro(spell)
end

function Attach (spell)
	Intro(spell)
	local tiles = spell.Get(spell.numTiles)

	for x = 0, spell.numShots-1 do

		if (spell.tiles.Count == 1) then
			spell.tiles = spell.Get(spell.numTiles)
		end

		spell.P.CreateBlastEffect(spell, tiles[0])
		
		-- local topRight = spell.P.CreateCustomShot(spell, tiles[0], false, spell.shotDuration, spell.damage)
		local attachedShot = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
		attachedShot.transform.SetParent(spell.being.transform)
		attachedShot.SetToGunPoint()

		if (spell.timeBetweenShots > 0) then
			WaitForSeconds(spell, spell.timeBetweenShots)
		end
	end
	
	Outro(spell)
end

function Trigun (spell)
	
	if (spell.fireAnim == true and spell.fireAnimLate == false) then
		if (spell.fireLoop == FireLoop.Normal) then
			spell.beingAnim.SetTrigger("fire")
		end
	end
	
	Intro(spell)

	local tiles = spell.Get(spell.numTiles)
	local shotNum = 0

	for x = 0, spell.numShots-1 do
		if (shotNum == 0) then
			local topRight = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
			topRight.SetVelocity(spell.shotVelocity, Direction.UpRight)
			topRight.SetToGunPoint()

		elseif (shotNum == 1) then
			local botRight = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
			botRight.SetVelocity(spell.shotVelocity, Direction.DownRight)
			botRight.SetToGunPoint()

		elseif (shotNum == 2) then
			local mid = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
			mid.SetVelocity(spell.shotVelocity, Direction.Right)
			mid.SetToGunPoint()
		end

		WaitForSeconds(spell, spell.timeBetweenShots)

		shotNum = shotNum + 1

		if (shotNum > 2) then
			shotNum = 0
		end

	end
	
	
	Outro(spell)
end

function Crossgun (spell)
	Intro(spell)
	local tiles = spell.Get(spell.numTiles)
	
	if (spell.fireAnim == true) then
		if (spell.fireLoop == FireLoop.Normal) then
			spell.beingAnim.SetTrigger("fire")
		end
	end

	local top = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
	top.SetVelocity(spell.shotVelocity, Direction.Up)
	top.SetToGunPoint()
	
	local right = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
	right.SetVelocity(spell.shotVelocity, Direction.Right)
	right.SetToGunPoint()

	local bot = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
	bot.SetVelocity(spell.shotVelocity, Direction.Down)
	bot.SetToGunPoint()

	local left = spell.P.CreateShot(spell, tiles[0].x, tiles[0].y)
	left.SetVelocity(spell.shotVelocity, Direction.Left)
	left.SetToGunPoint()
	
	Outro(spell)
end

function Shine (spell)
	Intro(spell)
	local tiles = spell.Get(spell.numTiles)
	local spawnDistance = spell.dashDistance
	local facingNum = spell.being.FacingDirection() * spawnDistance

	for i = 0, tiles.Count-1 do
	
		local shotNum = 0

		if (spell.HasParam("diagonal")) then
			if (shotNum == 0) then
				shotNum = 4
			end
		end

		for x = 0, spell.numShots-1 do
			if (shotNum == 0) then
				spell.P.CreateCustomShot(spell, tiles[i].x + facingNum, tiles[i].y + spawnDistance, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.DownLeft)
			elseif (shotNum == 1) then
				spell.P.CreateCustomShot(spell, tiles[i].x - facingNum, tiles[i].y - spawnDistance, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.UpRight)
			elseif (shotNum == 2) then
				spell.P.CreateCustomShot(spell, tiles[i].x - facingNum, tiles[i].y + spawnDistance, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.DownRight)
			elseif (shotNum == 3) then
				spell.P.CreateCustomShot(spell, tiles[i].x + facingNum, tiles[i].y - spawnDistance, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.UpLeft)
			elseif (shotNum == 4) then

				spell.P.CreateCustomShot(spell, tiles[i].x, tiles[i].y - spawnDistance, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.Up)
			elseif (shotNum == 5) then
				spell.P.CreateCustomShot(spell, tiles[i].x, tiles[i].y + spawnDistance, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.Down)
			elseif (shotNum == 6) then
				spell.P.CreateCustomShot(spell, tiles[i].x - facingNum, tiles[i].y, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.Right)
			elseif (shotNum == 7) then
				spell.P.CreateCustomShot(spell, tiles[i].x + facingNum, tiles[i].y, false, spell.shotDuration, spell.damage, 0, spell.animShot, spell.shotSound).SetVelocity(spell.shotVelocity, Direction.Left)
			end

			if (spell.timeBetweenShots > 0) then
				WaitForSeconds(spell, spell.timeBetweenShots)
			end

			shotNum = shotNum + 1

			if (spell.HasParam("cross")) then
				if (shotNum > 3) then
					shotNum = 0
				end
			end
	
			if (shotNum > 7) then
				shotNum = 0
			end
		end

	end
	
	Outro(spell)
end

function Shotgun (spell)
	Intro(spell)
	local tiles = spell.Get(spell.numTiles)

	spell.P.CreateCastEffect (spell, tiles[0])
	
	WaitForSeconds(spell, spell.castDelay)

	local shotNum = 0
	for x = 0, spell.numShots-1 do
		if (shotNum == 0) then
			spell.P.CreateCustomShot(spell, tiles[0].x + 1*spell.being.FacingDirection(), tiles[0].y + 0, false, spell.shotDuration, spell.damage, 0, spell.animShot)

			spell.P.CreateCustomShot(spell, tiles[0].x + 2*spell.being.FacingDirection(), tiles[0].y + 1, false, spell.shotDuration, spell.damage, 0, spell.animShot)
		elseif (shotNum == 1) then

			spell.P.CreateCustomShot(spell, tiles[0].x + 1*spell.being.FacingDirection(), tiles[0].y + 0, false, spell.shotDuration, spell.damage, 0, spell.animShot)
			spell.P.CreateCustomShot(spell, tiles[0].x + 2*spell.being.FacingDirection(), tiles[0].y + 0, false, spell.shotDuration, spell.damage, 0, spell.animShot)
		elseif (shotNum == 2) then
			spell.P.CreateCustomShot(spell, tiles[0].x + 1*spell.being.FacingDirection(), tiles[0].y + 0, false, spell.shotDuration, spell.damage, 0, spell.animShot)
			spell.P.CreateCustomShot(spell, tiles[0].x + 2*spell.being.FacingDirection(), tiles[0].y - 1, false, spell.shotDuration, spell.damage, 0, spell.animShot)
		end

		WaitForSeconds(spell, spell.timeBetweenShots)

		shotNum = shotNum + 1

		if (shotNum > 2) then
			shotNum = 0
		end
	end
	
	Outro(spell)
end

function CastAll (spell)
	while (spell.being.player.duelDisk.shuffling == false) do
		WaitForSeconds(spell, 0.2)
		while (spell.being.player.mov.state ~= State.Idle) do
			WaitForSeconds(spell, 0)
		end
		spell.being.player.CastSpell(0, 0, true)
	end
end

function CastOther (spell)
	WaitForSeconds(spell, 0)
	local slotToCast = 0
	if (spell.castSlotNum == 0) then
		slotToCast = 1
	end
	spell.being.player.CastSpell(slotToCast, 0, true)
end

function SpikeTrail (spell)
	Intro(spell)
	
	if (spell.fireAnim == true) then
		spell.beingAnim.SetTrigger ("fire")
	end

	local tiles = spell.Get(spell.numTiles)

	for i = 0, tiles.Count-1 do
	
		local tempX = tiles[i].x
		local tempY = tiles[i].y
		local rand = 1

		while (tempX <= 7 and tempX >= 0) do

			local newAtk = spell.P.CreateLerper (spell, tempX, tempY, false)
			local newEffect = spell.P.CreateBlastEffect (spell, tempX, tempY, false)

			rand = rand + 1

			local target = spell.ctrl.currentPlayer

			if (spell.being.player ~= nil) then
				target = spell.being.player
			end

			if (target.mov.currentTile.y > newAtk.mov.currentTile.y) then
				tempY = tempY + 1
			elseif (target.mov.currentTile.y < newAtk.mov.currentTile.y) then
				tempY = tempY - 1
			end

			tempX = tempX + spell.being.FacingDirection()
			if (tempX <= 7 and tempX >= 0) then
				spell.P.CreateWarning (spell, tempX, tempY, spell.timeBetweenShots, false)
			end

			WaitForSeconds(spell, spell.timeBetweenShots)
		end
	end

	WaitForSeconds(spell, 1)

	Outro(spell)
end


function Reaper (spell)
	-- vanis anim
	spell.being.dontInterruptAnim = true

	for x = 0, spell.numShots-1 do

		spell.tiles = spell.Get(spell.numTiles)
	
		for i = 0, spell.tiles.Count-1 do

			local xPosChange = 1
			if (spell.tiles[i].x > 0 and spell.Param("reaperPos") == "back") then
				xPosChange = -1
			end

			-- print(xPosChange)

			spell.being.PlayOnce(spell.introSound)
			spell.being.mov.MoveTo (spell.tiles[i].x + xPosChange, spell.tiles[i].y, true, false, true, false, false, false)


			spell.beingAnim.SetBool ("dashing", true)
			WaitWhileMoving (spell)
			spell.beingAnim.SetBool ("dashing", false)

			spell.being.mov.SetState(State.Attacking)
			
			spell.beingAnim.SetTrigger ("lift")

			WaitForSeconds(spell, spell.castDelay)

			if (spell.fireAnim == true) then
				spell.beingAnim.SetTrigger ("fire")
			end

			local newAnimBlast = spell.P.CreateBlastEffect (spell, spell.slashLocation, 0, true, spell.shotVelocity, true)

			spell.P.CreateShot (spell, spell.slashLocation, 0, true)

			WaitForSeconds(spell, spell.timeBetweenShots)
		end
	end

	-- spell.beingAnim.SetTrigger ("toIdle") --For cpu

	-- spell.being.mov.MoveTo (spell.being.mov.currentTile.x, spell.being.mov.currentTile.y, false, false)
	spell.being.mov.MoveTo (spell.being.mov.currentTile.x, spell.being.mov.currentTile.y, false, false, false, false, true, true, true)

	WaitWhileMoving(spell)

	Outro(spell)
	spell.being.dontInterruptAnim = false
end

function Tracker (spell)
	
	Intro(spell)

	local playerMov = spell.ctrl.currentPlayer.mov

	local oldTile = playerMov.currentTile
	local tileHistory = {}
	local timeHistory = {}
	local timeOnTile = 0
	local currentTrackTime = spell.Time.time + 2

	-- tileHistory.Add (playerMov.currentTile)
	

	
	local firstLerper = spell.P.CreateWarning(spell, playerMov.endTile.x, playerMov.endTile.y, spell.shotDuration)
	firstLerper.transform.position = spell.ctrl.currentPlayer.transform.position
	firstLerper.transform.SetParent (playerMov.transform)
	local trailer = spell.P.CreateCustomShot(spell, playerMov.endTile.x, playerMov.endTile.y, false, 10, 0)
	-- TrailRenderer trailer = Instantiate (thisTrailer, ctrl.currentPlayer.transform.position, transform.rotation) as TrailRenderer
	-- trailer.transform.SetParent(ctrl.transform)



	print ("StartingWhile")

	-- spell.spell.beingAnim.SetTrigger ("lift")
	-- tileHistory.Add (playerMov.currentTile)
	tileHistory[0] = playerMov.currentTile
	timeHistory[0] = 0
	-- timeHistory.Add (0)

	local historyCount = 1

	while (currentTrackTime > spell.Time.time) do

		timeOnTile =  timeOnTile + spell.Time.deltaTime
		firstLerper.transform.position = spell.ctrl.currentPlayer.transform.position
		trailer.transform.position = spell.ctrl.currentPlayer.transform.position

		if (playerMov.currentTile ~= oldTile) then
			tileHistory[historyCount] = playerMov.currentTile
			timeHistory[historyCount] = timeOnTile
			-- print ("Added tile and time: " + timeOnTile)

			timeOnTile = 0
			print (historyCount)

			oldTile = playerMov.currentTile
			historyCount = historyCount + 1
		end

		WaitForSeconds(spell, 0)
	end

	tileHistory[historyCount] = playerMov.currentTile
	timeHistory[historyCount] = timeOnTile

	print ("EndedScanner")

	local newLerper= spell.P.CreateLerper (spell, tileHistory[0].x, tileHistory[0].y, true)

	-- lineRend.enabled = true
	-- lineRend.positionCount = 2
	-- //lineRend.transform.SetParent (newLerper.transform)
	-- lineRend.colorGradient = laserGradient
	-- lineRend.material = GetComponent<LineRenderer> ().material
	-- lineRend.startWidth = 2

	-- lineRend.SetPosition (0, being.gunPoint.position)
	-- lineRend.SetPosition (1, newLerper.transform.position)

	-- spell.beingAnim.SetTrigger ("fire")

	local currentLaserTime = spell.Time.time
	-- print (table.getn(tileHistory))

	for i = 0, table.getn(tileHistory)-1 do
	print ("x point")

		currentLaserTime = currentLaserTime + timeHistory[i]
		WaitForSeconds(spell, currentLaserTime - spell.Time.time)
		-- yield return new WaitUntil (() => currentLaserTime <= Time.time)

		if (newLerper.isActiveAndEnabled) then
			-- print ("Movement: " + i)
			newLerper.mov.MoveToTile (tileHistory [i + 1], false, false, true)
		end

		while (newLerper.mov.state ~= State.Idle) do
			if (newLerper.isActiveAndEnabled) then
				-- lineRend.SetPosition (0, being.gunPoint.position)
				-- lineRend.SetPosition (1, newLerper.transform.position)
			else
				break
			end
			WaitForSeconds(spell, 0)
		end
	end

	-- lineRend.enabled = false

	spell.being.mov.state = State.Idle
end

function RemoveOccupation (spell)
	spell.being.mov.currentTile.SetOccupation(0)
end


function Timerbar (spell)
	-- spell.being.mov.SetState(State.Attacking)
		
	if (spell.being.HasParameter("charge")) then
		spell.beingAnim.SetTrigger("charge")
	end

	spell.being.AddTimerBar(tonumber(spell.Param("closetime")))
end

function AddArtifactLoot (spell) -- queue an artifact reward for the next rewards screen

	for x = 0, spell.numShots-1 do -- For getting actual artifacts
		if (spell.HasParam("rarity")) then
			local rarity = tonumber(spell.Param("rarity"))
			spell.being.beingObj.rewardList.Add(spell.deCtrl.itemMan.GetItemIDs(rarity, 1, ItemType.Art)[0])
		else
			spell.being.beingObj.rewardList.Add(spell.deCtrl.itemMan.GetItemIDs(-1, 1, ItemType.Art)[0])
		end
	end
end

function AddOrbLoot (spell)

	for x = 0, spell.numShots-1 do
		local rarity = -1

		if (spell.HasParam("rarity")) then
			rarity = tonumber(spell.Param("rarity"))
		else
			rarity = -1
		end

		spell.being.AddOrb(rarity)

	end

	
end

function AddSpellLoot (spell)
	if (spell.HasParam("rarity")) then
		local rarity = tonumber(spell.Param("rarity"))
		spell.being.beingObj.rewardList = spell.deCtrl.itemMan.GetItemIDs(rarity, 1, ItemType.Spell)
	end
end

function SetMoney (spell)
	if (spell.HasParam("amount")) then
		local amount = tonumber(spell.Param("amount"))
		spell.being.beingObj.money = amount
	end
end

function DropItems (spell)
	spell.being.DropItems()
end

function ChangeZone (spell)
	WaitForSeconds(spell, spell.castDelay)
	spell.ctrl.runCtrl.ChangeZone (spell.numTiles, 0, spell.description)
end

function ChangeWorld (spell)
	spell.being.PlayOnce(spell.introSound)
	
	if (spell.being.health.current == spell.damage) then
		WaitForSeconds(spell, spell.castDelay)
		spell.ctrl.runCtrl.ChangeWorld (tonumber(spell.Param("worldNum")))
		spell.beingAnim.SetTrigger("unlock")
	end
end

function CloseShop (spell)
	spell.ctrl.shopCtrl.Close (true)
end

function MessagePop (spell)
	spell.being.talkBubble.AnimateText(spell.Param("messageText"),tonumber(spell.Param("messageDuration")))
	if (spell.HasParam("resetLift")) then
		spell.beingAnim.SetTrigger("resetLift")
	end
end

function RandomMessagePop (spell)
	spell.being.talkBubble.AnimateRandomLine(spell.Param("messageText"))
	if (spell.HasParam("resetLift")) then
		spell.beingAnim.SetTrigger("resetLift")
	end
end

function LootChest (spell)
	Timerbar(spell)
end

function ClearSelf (spell)
	WaitForSeconds(spell, tonumber(spell.Param("clearDelay")))
	
	if (spell.being.inDeathSequence) then 
		return
	end
	if (spell.being.dead == false) then
		spell.being.col.enabled = false
		spell.being.Clear()
	end
end

function ClearSelfBeingDelay (spell)
	WaitForSeconds(spell, spell.being.beingObj.clearDelay)
	
	if (spell.being.inDeathSequence) then 
		return
	end
	if (spell.being.dead == false) then
		spell.being.col.enabled = false
		spell.being.Clear()
	end
end

function FakeDown (spell)
	if (spell.being.player) then
		spell.being.player.dontInterruptAnim = true
	end
	WaitForSeconds(spell, spell.castDelay)

	if (spell.being.player ~= nil) then

		spell.being.player.AddControlBlock(Block.Fake)
		spell.being.player.ClearQueuedActions()
	end

	if (spell.being) then
		spell.being.anim.SetTrigger("down")
	end
	WaitForSeconds(spell, spell.shotDuration)

	if (spell.being.player ~= nil) then
		spell.being.player.dontInterruptAnim = false
		spell.being.player.RemoveControlBlock(Block.Fake)
        spell.being.anim.SetTrigger("undown");
	end
end

function UnoccupyTile (spell)
	spell.being.mov.currentTile.SetOccupation(0)
end

function SpecialAnim (spell)
	spell.beingAnim.SetTrigger("specialStart")
	WaitForSeconds(spell, 0.6)
	spell.beingAnim.SetTrigger("specialEnd")
end

function ChargeAnim (spell)
	spell.beingAnim.SetTrigger("charge")
	WaitForSeconds(spell, 0.3)
	spell.beingAnim.SetTrigger("release")
end

function ThrowAnim (spell)
	spell.beingAnim.SetTrigger("throw")
end

function Message (spell)
	Intro(spell)
	spell.being.CreateFloatText (
		spell.ctrl.statusText, spell.Param("messageText"),
		tonumber(spell.Param("messageX")),
		tonumber(spell.Param("messageY")),
		tonumber(spell.Param("messageDuration")),
		nil,
		tonumber(spell.Param("messageSize"))
	)
	Outro(spell)
end

function HostageSaved (spell)
	spell.ctrl.HostageSaved()
end

function HostageKilled (spell)
	spell.ctrl.HostageKilled()
end

function Darkness (spell)

	spell.being.battleGrid.Darkness(spell.tiles.Count*spell.timeBetweenShots+0.2, 0.1)
end

function StartChannel(spell)
	if (spell.anchor == false) then
		if (spell.channel) then
			return spell.being.mov.CreateChannel()
		else
			if (spell.interrupt) then
				spell.being.mov.SetState(State.Idle)
			end
		end
	else
		spell.being.mov.SetState(State.Attacking)
	end
	return 0
end

function Intro (spell)
	if (spell.introSound) then
		spell.being.PlayOnce(spell.introSound)
	end
	
	if (spell.anchor == true) then
		spell.being.mov.SetState(State.Attacking)
	end

	if (spell.castDelay > 0) then
		WaitForSeconds(spell, spell.castDelay)
	end
	
	if (spell.fireLoop == FireLoop.While) then
		spell.beingAnim.SetBool("fireWhile", true)
	end
end

function Outro (spell, storedBeing)
	if (storedBeing == nil) then
		storedBeing = spell.being
	end

	if (spell.anchor == true) then
		storedBeing.mov.SetState(State.Idle)
	end
	
	if (spell.fireLoop == FireLoop.While) then
		spell.beingAnim.SetBool("fireWhile", false)
	end
end

function Blank()

end
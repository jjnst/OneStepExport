function WaitForSeconds(item, seconds)
	local timer = 0

	while true do
		timer = timer + item.GetDeltaTime()
		coroutine.yield(timer)

		if (timer > seconds) then
			break
		end
	end
	-- local frames = seconds*60
	-- WaitForFrames(frames-1)
end

function WaitForFrames(frames)
	local r =  0

	while true do
		r = r + 1
		coroutine.yield(r)
		if (r >= frames) then
			break
		end
	end
end

function WaitUntil(being, state)
	while true do
		coroutine.yield()
		
		if (being.mov.state == state) then
			break
		end
	end
end

function WaitWhileMoving(spell)
	WaitWhileBeingState(spell.being, State.Moving)
end

function WaitWhileBeingState(being, state)
	-- being.mov.lerpTime = 0.12
	
	while true do
		coroutine.yield()
		
		if (being.mov.state ~= state) then
			break
		end
	end
	-- being.mov.lerpTime = 0.1
end

function PlaceStatusNoDuration(item, status)
	for _, target in ipairs(GetTarget(item)) do
		target.AddStatus(status, GetAmount(item), 0, item)
	end
end

function PlaceStatusWithDuration(item, status)
	for _, target in ipairs(GetTarget(item)) do
		target.AddStatus(status, GetAmount(item), GetDuration(item), item)
	end
end

function GetAmount(item)
	return item.ctrl.GetAmount(item)
end

function GetRoundedAmount(item)
	return math.floor(item.ctrl.GetAmount(item))
end

function GetDuration(item)
	return item.ctrl.GetDuration(item)
end

function GetRoundedDuration(item)
	return math.floor(item.ctrl.GetDuration(item))
	-- return math.floor(item.ctrl.GetAmount(item.currentApp.durationApp, item.currentApp.duration, item.spellObj, item.artObj, item.pactObj))
end

function SummonAfter (item, tile, delay, savedApp) 
	item.item.SummonAfter (item, tile, delay, savedApp)
end

function GetTarget (item)
	return item.ctrl.GetTargets(item)
end
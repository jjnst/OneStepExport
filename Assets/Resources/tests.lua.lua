-- Built-in types test

defineBuiltInTypesObject{
	myBool = true,
	myInt = -376,
	myFloat = 253.84023,
	myDouble = -89320.234234,
	myString = "Hello!",
	myByte = 123,
	myDecimal = 388402384.234789,
	myIntArray = {1, 2, 3, 4, 5},
	myDictionaryListArray = {
		string1 = {
			{4, 3, 2},
			{4, 5},
			{9, 4, 24},
		},
		string2 = {
			{5, 3, 18, 43, 2890, -34, 45},
			{2},
			{},
			{90, 3, 4, 5},
		},
	},
}

-- Unity Types test

local i = 20

defineUnityTypesObject{
	myColor = {0.1, 0.5, 0.3, 1},
	myColor32 = {255, 135, 178, 90},
	myRect = {50, 20, 80, 145},
	myVector2 = {23, 56.3},
	myVector3 = {2.5, 56.3, 0.032},
	myVector4 = {1, 50, 34, 2.6},
	myVector3Array = {
		{4, 3, i + 6},
		{0.4, 42, i + 7},
		{2, 6, i + 9},
		{45, 90}, -- Test vector3 defined by only 2 members
	},
	myColorList = {
		{0.4, 0.5, 1}, -- Test Color without specified alpha
		{0.3, 0.9, 2, 0.1},
		{0.5, 0.9, 0.4, 1},
	},
	myRectDictionary = {
		rect1 = {10, 10, 200, 200},
		rect2 = {30, 35, 150, 300},
	},
}

-- Nested classes test

defineEnemies{
	bandit = {
		health = 30,
		attacks = {
			punch = {
				power = 5,
				cooldown = 3.2,
			},
			kick = {
				power = 9,
				cooldown = 4.2,
			},
		},
	},
	crow = {
		health = 12,
		flying = true,
		attacks = {
			claw = {
				power = 4,
				cooldown = 2.3,
			},
		},
	},
}
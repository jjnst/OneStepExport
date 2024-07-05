using System.Collections.Generic;
using UnityEngine;

namespace LuaFramework.Tests
{
	public class UnityTypesObject
	{
		public Color myColor { get; set; }

		public Color32 myColor32 { get; set; }

		public Rect myRect { get; set; }

		public Vector2 myVector2 { get; set; }

		public Vector3 myVector3 { get; set; }

		public Vector4 myVector4 { get; set; }

		public Vector3[] myVector3Array { get; set; }

		public List<Color> myColorList { get; set; }

		public Dictionary<string, Rect> myRectDictionary { get; set; }
	}
}

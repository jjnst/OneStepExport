using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using UnityEngine;

namespace LuaFramework
{
	public static class LuaReader
	{
		private static readonly Dictionary<Type, Func<DynValue, object>> customReaders = new Dictionary<Type, Func<DynValue, object>>();

		public static T Read<T>(Table luaTable)
		{
			return (T)Convert(DynValue.NewTable(luaTable), typeof(T));
		}

		public static T Read<T>(DynValue luaValue)
		{
			return (T)Convert(luaValue, typeof(T));
		}

		private static void ReadProperty<T>(T obj, string propertyName, DynValue propertyValue)
		{
			try
			{
				PropertyInfo property = obj.GetType().GetProperty(propertyName);
				if (property != null && property.CanWrite && propertyValue != null)
				{
					Type propertyType = property.PropertyType;
					property.SetValue(obj, Convert(propertyValue, propertyType), null);
				}
			}
			catch
			{
				Debug.Log("LuaReader: could not define property \"" + propertyName + "\".");
				throw new Exception();
			}
		}

		private static object Convert(DynValue luaValue, Type type)
		{
			if (customReaders.ContainsKey(type))
			{
				return customReaders[type](luaValue);
			}
			if (type == typeof(bool))
			{
				return luaValue.Boolean;
			}
			if (type == typeof(int))
			{
				return (int)luaValue.Number;
			}
			if (type == typeof(float))
			{
				return (float)luaValue.Number;
			}
			if (type == typeof(double))
			{
				return luaValue.Number;
			}
			if (type == typeof(string) && luaValue.String != null)
			{
				return luaValue.String;
			}
			if (type == typeof(byte))
			{
				return (byte)luaValue.Number;
			}
			if (type == typeof(decimal))
			{
				return (decimal)luaValue.Number;
			}
			if (luaValue.Type == DataType.Function)
			{
				return luaValue.Function;
			}
			if (type.IsEnum && luaValue.String != null)
			{
				return System.Convert.ChangeType(Enum.Parse(type, luaValue.String), type);
			}
			if (luaValue.Table == null)
			{
				return null;
			}
			if (type == typeof(Color))
			{
				return ReadColor(luaValue.Table);
			}
			if (type == typeof(Color32))
			{
				return ReadColor32(luaValue.Table);
			}
			if (type == typeof(Rect))
			{
				return ReadRect(luaValue.Table);
			}
			if (type == typeof(Vector2))
			{
				return ReadVector2(luaValue.Table);
			}
			if (type == typeof(Vector3))
			{
				return ReadVector3(luaValue.Table);
			}
			if (type == typeof(Vector4))
			{
				return ReadVector4(luaValue.Table);
			}
			if (type.IsArray)
			{
				return ReadArray(luaValue.Table, type);
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				return ReadList(luaValue.Table, type);
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
			{
				return ReadDictionary(luaValue.Table, type);
			}
			if (type.IsClass)
			{
				return ReadClass(luaValue.Table, type);
			}
			return null;
		}

		private static Color ReadColor(Table luaTable)
		{
			float r = ((luaTable[1] == null) ? 0f : ((float)(double)luaTable[1]));
			float g = ((luaTable[2] == null) ? 0f : ((float)(double)luaTable[2]));
			float b = ((luaTable[3] == null) ? 0f : ((float)(double)luaTable[3]));
			float a = ((luaTable[4] == null) ? 1f : ((float)(double)luaTable[4]));
			return new Color(r, g, b, a);
		}

		private static Color32 ReadColor32(Table luaTable)
		{
			byte r = (byte)((luaTable[1] != null) ? ((byte)(double)luaTable[1]) : 0);
			byte g = (byte)((luaTable[2] != null) ? ((byte)(double)luaTable[2]) : 0);
			byte b = (byte)((luaTable[3] != null) ? ((byte)(double)luaTable[3]) : 0);
			byte a = ((luaTable[4] == null) ? byte.MaxValue : ((byte)(double)luaTable[4]));
			return new Color32(r, g, b, a);
		}

		private static Rect ReadRect(Table luaTable)
		{
			float x = ((luaTable[1] == null) ? 0f : ((float)(double)luaTable[1]));
			float y = ((luaTable[2] == null) ? 0f : ((float)(double)luaTable[2]));
			float width = ((luaTable[3] == null) ? 0f : ((float)(double)luaTable[3]));
			float height = ((luaTable[4] == null) ? 0f : ((float)(double)luaTable[4]));
			return new Rect(x, y, width, height);
		}

		private static Vector2 ReadVector2(Table luaTable)
		{
			float x = ((luaTable[1] == null) ? 0f : ((float)(double)luaTable[1]));
			float y = ((luaTable[2] == null) ? 0f : ((float)(double)luaTable[2]));
			return new Vector2(x, y);
		}

		private static Vector3 ReadVector3(Table luaTable)
		{
			float x = ((luaTable[1] == null) ? 0f : ((float)(double)luaTable[1]));
			float y = ((luaTable[2] == null) ? 0f : ((float)(double)luaTable[2]));
			float z = ((luaTable[3] == null) ? 0f : ((float)(double)luaTable[3]));
			return new Vector3(x, y, z);
		}

		private static Vector4 ReadVector4(Table luaTable)
		{
			float x = ((luaTable[1] == null) ? 0f : ((float)(double)luaTable[1]));
			float y = ((luaTable[2] == null) ? 0f : ((float)(double)luaTable[2]));
			float z = ((luaTable[3] == null) ? 0f : ((float)(double)luaTable[3]));
			float w = ((luaTable[4] == null) ? 0f : ((float)(double)luaTable[4]));
			return new Vector4(x, y, z, w);
		}

		private static Array ReadArray(Table luaTable, Type type)
		{
			Type elementType = type.GetElementType();
			if (elementType == null)
			{
				return null;
			}
			if (type.GetArrayRank() == 1)
			{
				Array array = Array.CreateInstance(elementType, luaTable.Values.Count());
				int num = 0;
				foreach (DynValue value in luaTable.Values)
				{
					array.SetValue(System.Convert.ChangeType(Convert(value, elementType), elementType), num);
					num++;
				}
				return array;
			}
			if (type.GetArrayRank() == 2)
			{
				int length = (from dynValue in luaTable.Values
					where dynValue.Table != null
					select dynValue.Table.Values.Count()).Concat(new int[1]).Max();
				Array array2 = Array.CreateInstance(elementType, luaTable.Values.Count(), length);
				int num2 = 0;
				foreach (DynValue value2 in luaTable.Values)
				{
					int num3 = 0;
					foreach (DynValue value3 in value2.Table.Values)
					{
						array2.SetValue(System.Convert.ChangeType(Convert(value3, elementType), elementType), num2, num3);
						num3++;
					}
					num2++;
				}
				return array2;
			}
			return null;
		}

		private static IList ReadList(Table luaTable, Type type)
		{
			Type type2 = type.GetGenericArguments()[0];
			IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type2));
			foreach (DynValue value in luaTable.Values)
			{
				list.Add(System.Convert.ChangeType(Convert(value, type2), type2));
			}
			return list;
		}

		private static IDictionary ReadDictionary(Table luaTable, Type type)
		{
			Type type2 = type.GetGenericArguments()[0];
			Type type3 = type.GetGenericArguments()[1];
			IDictionary dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<, >).MakeGenericType(type2, type3));
			foreach (TablePair pair in luaTable.Pairs)
			{
				object obj = System.Convert.ChangeType(Convert(pair.Key, type2), type2);
				if (obj != null)
				{
					dictionary[obj] = System.Convert.ChangeType(Convert(pair.Value, type3), type3);
				}
			}
			return dictionary;
		}

		private static object ReadClass(Table luaTable, Type type)
		{
			object obj = Activator.CreateInstance(type);
			ReadClassData(obj, luaTable);
			return obj;
		}

		public static void ReadClassData<T>(T clrObject, DynValue luaValue)
		{
			ReadClassData(clrObject, luaValue.Table);
		}

		public static void ReadClassData<T>(T clrObject, Table luaTable)
		{
			if (!typeof(T).IsValueType && clrObject == null)
			{
				Debug.LogWarning("LuaReader: method ReadObjectData called with null object.");
				return;
			}
			if (luaTable == null)
			{
				Debug.LogWarning("LuaReader: method ReadObjectData called with null Lua table.");
				return;
			}
			foreach (TablePair pair in luaTable.Pairs)
			{
				ReadProperty(clrObject, pair.Key.String, pair.Value);
			}
		}

		public static void ReadSingleProperty<T>(T clrObject, string propertyName, Table luaTable)
		{
			if (clrObject == null)
			{
				Debug.LogWarning("LuaReader: method ReadSingleProperty called with null object.");
			}
			else if (luaTable == null)
			{
				Debug.LogWarning("LuaReader: method ReadSingleProperty called with null Lua table.");
			}
			else
			{
				ReadProperty(clrObject, propertyName, luaTable.Get(propertyName));
			}
		}

		public static void AddCustomReader(Type type, Func<DynValue, object> reader)
		{
			if (!(type == null) && reader != null)
			{
				customReaders[type] = reader;
			}
		}

		public static void RemoveCustomReader(Type type)
		{
			if (!(type == null))
			{
				customReaders.Remove(type);
			}
		}
	}
}

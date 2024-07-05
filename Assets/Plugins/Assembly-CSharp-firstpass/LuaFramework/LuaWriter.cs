using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LuaFramework
{
	public class LuaWriter : StringWriter
	{
		private Type _requiredAttributeType;

		private int _tabs;

		private bool _waitingForTabs = true;

		public void AddTab()
		{
			_tabs++;
		}

		public void RemoveTab()
		{
			_tabs--;
			if (_tabs < 0)
			{
				_tabs = 0;
			}
		}

		public override void Write(string str)
		{
			if (_waitingForTabs)
			{
				WriteTabs();
				_waitingForTabs = false;
			}
			base.Write(str);
		}

		public override void WriteLine()
		{
			if (_waitingForTabs)
			{
				WriteTabs();
				_waitingForTabs = false;
			}
			base.WriteLine();
			_waitingForTabs = true;
		}

		private void WriteTabs()
		{
			for (int i = 0; i < _tabs; i++)
			{
				base.Write("\t");
			}
		}

		public void SetRequiredAttribute(Type attributeType)
		{
			_requiredAttributeType = attributeType;
		}

		public void WriteProperty<T>(T obj, string propertyName, bool multiline = true, bool trailingComma = false)
		{
			PropertyInfo property = obj.GetType().GetProperty(propertyName);
			if (property == null)
			{
				Debug.Log("ERROR " + propertyName);
			}
			WriteProperty(obj, property, multiline, trailingComma);
		}

		private void WriteProperty<T>(T obj, PropertyInfo propertyInfo, bool multiline = true, bool trailingComma = false)
		{
			object value = propertyInfo.GetValue(obj, null);
			Type propertyType = propertyInfo.PropertyType;
			if (value != null)
			{
				Write(propertyInfo.Name + " = ");
				WriteObject(value, propertyType, multiline, trailingComma);
			}
		}

		public void WriteObject<T>(T obj, bool multiline = true, bool trailingComma = false)
		{
			WriteObject(obj, typeof(T), multiline, trailingComma);
		}

		private void WriteObject(object obj, Type type, bool multiline = true, bool trailingComma = false)
		{
			string[] array = new string[2] { " sdf", "sdf" };
			if (type == typeof(bool))
			{
				Write(((bool)obj).ToString().ToLower());
			}
			else if (type == typeof(int))
			{
				Write(((int)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			}
			else if (type == typeof(float))
			{
				Write(((float)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			}
			else if (type == typeof(double))
			{
				Write(((double)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			}
			else if (type == typeof(string))
			{
				Write(((string)obj).ToLiteral());
			}
			else if (type == typeof(byte))
			{
				Write(((byte)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			}
			else if (type == typeof(decimal))
			{
				Write(((decimal)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			}
			else if (type.IsEnum)
			{
				Write(obj.ToString().ToLiteral());
			}
			else if (type == typeof(Color))
			{
				WriteColor((Color)obj);
			}
			else if (type == typeof(Color32))
			{
				WriteColor32((Color32)obj);
			}
			else if (type == typeof(Rect))
			{
				WriteRect((Rect)obj);
			}
			else if (type == typeof(Vector2))
			{
				WriteVector2((Vector2)obj);
			}
			else if (type == typeof(Vector3))
			{
				WriteVector3((Vector3)obj);
			}
			else if (type == typeof(Vector4))
			{
				WriteVector4((Vector4)obj);
			}
			else if (type.IsArray)
			{
				WriteArray((Array)obj, multiline);
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				WriteList((IList)obj, multiline);
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
			{
				WriteDictionary((IDictionary)obj, multiline);
			}
			else if (type.IsClass)
			{
				WriteClass(obj, multiline);
			}
			if (trailingComma)
			{
				Write(", ");
				if (multiline)
				{
					WriteLine();
				}
			}
		}

		private void WriteColor(Color color)
		{
			WriteArray(new float[4] { color.r, color.g, color.b, color.a }, false);
		}

		private void WriteColor32(Color32 color32)
		{
			WriteArray(new byte[4] { color32.r, color32.g, color32.b, color32.a }, false);
		}

		private void WriteRect(Rect rect)
		{
			WriteArray(new float[4] { rect.x, rect.y, rect.width, rect.height }, false);
		}

		private void WriteVector2(Vector2 vector2)
		{
			WriteArray(new float[2] { vector2.x, vector2.y }, false);
		}

		private void WriteVector3(Vector3 vector3)
		{
			WriteArray(new float[3] { vector3.x, vector3.y, vector3.z }, false);
		}

		private void WriteVector4(Vector4 vector4)
		{
			WriteArray(new float[4] { vector4.x, vector4.y, vector4.z, vector4.w }, false);
		}

		private void WriteDictionary(IDictionary dictionary, bool multiline = true)
		{
			Write("{");
			if (multiline)
			{
				WriteLine();
			}
			AddTab();
			foreach (object key in dictionary.Keys)
			{
				string text = key as string;
				if (text != null)
				{
					if (IsValidTableString(text))
					{
						Write(text + " = ");
					}
					else
					{
						Write("[\"" + text + "\"] = ");
					}
				}
				else if (key is int)
				{
					Write("[" + (int)key + "] = ");
				}
				WriteObject(dictionary[key], dictionary.GetType().GetGenericArguments()[1], multiline, true);
			}
			RemoveTab();
			Write("}");
		}

		private bool IsValidTableString(string key)
		{
			return Regex.IsMatch(key, "^\\D\\w*$");
		}

		private void WriteArray(Array array, bool multiline = true)
		{
			Write("{");
			if (multiline)
			{
				WriteLine();
			}
			AddTab();
			if (array.Rank == 1)
			{
				int length = array.GetLength(0);
				for (int i = 0; i < length; i++)
				{
					WriteObject(array.GetValue(i), array.GetType().GetElementType(), multiline, i < length - 1);
				}
			}
			else if (array.Rank == 2)
			{
				int length2 = array.GetLength(0);
				int length3 = array.GetLength(1);
				for (int j = 0; j < length2; j++)
				{
					WriteLine("{");
					AddTab();
					for (int k = 0; k < length3; k++)
					{
						WriteObject(array.GetValue(j, k), array.GetType().GetElementType(), multiline, k < length3 - 1);
					}
					RemoveTab();
					Write("}");
					if (j < length2 - 1)
					{
						WriteLine(",");
					}
					else
					{
						WriteLine();
					}
				}
			}
			RemoveTab();
			Write("}");
		}

		private void WriteList(IList list, bool multiline = true)
		{
			Write("{");
			if (multiline)
			{
				WriteLine();
			}
			AddTab();
			foreach (object item in list)
			{
				WriteObject(item, list.GetType().GetGenericArguments()[0], multiline, true);
			}
			RemoveTab();
			Write("}");
		}

		private void WriteClass<T>(T obj, bool multiline = true, bool trailingComma = false)
		{
			Write("{");
			if (multiline)
			{
				WriteLine();
			}
			AddTab();
			ICustomLuaSerializer customLuaSerializer = obj as ICustomLuaSerializer;
			if (customLuaSerializer != null)
			{
				customLuaSerializer.Serialize(this);
			}
			else
			{
				PropertyInfo[] properties = obj.GetType().GetProperties();
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (_requiredAttributeType != null)
					{
						object[] customAttributes = propertyInfo.GetCustomAttributes(_requiredAttributeType, false);
						if (customAttributes.Length == 0)
						{
							continue;
						}
					}
					WriteProperty(obj, propertyInfo, multiline, true);
				}
			}
			RemoveTab();
			Write("}");
			if (trailingComma)
			{
				Write(", ");
				if (multiline)
				{
					WriteLine();
				}
			}
		}
	}
}

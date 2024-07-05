using System.Globalization;
using System.Text;

namespace LuaFramework
{
	public static class LuaWriterExtensionMethods
	{
		public static string ToLiteral(this string input)
		{
			StringBuilder stringBuilder = new StringBuilder(input.Length + 2);
			stringBuilder.Append("\"");
			foreach (char c in input)
			{
				switch (c)
				{
				case '\'':
					stringBuilder.Append("\\'");
					continue;
				case '"':
					stringBuilder.Append("\\\"");
					continue;
				case '\\':
					stringBuilder.Append("\\\\");
					continue;
				case '\0':
					stringBuilder.Append("\\0");
					continue;
				case '\a':
					stringBuilder.Append("\\a");
					continue;
				case '\b':
					stringBuilder.Append("\\b");
					continue;
				case '\f':
					stringBuilder.Append("\\f");
					continue;
				case '\n':
					stringBuilder.Append("\\n");
					continue;
				case '\r':
					stringBuilder.Append("\\r");
					continue;
				case '\t':
					stringBuilder.Append("\\t");
					continue;
				case '\v':
					stringBuilder.Append("\\v");
					continue;
				}
				if (char.GetUnicodeCategory(c) != UnicodeCategory.Control)
				{
					stringBuilder.Append(c);
					continue;
				}
				ushort num = c;
				stringBuilder.Append(num.ToString("x4"));
			}
			stringBuilder.Append("\"");
			return stringBuilder.ToString();
		}
	}
}

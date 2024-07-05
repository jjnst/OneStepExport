using MoonSharp.Interpreter.Serialization.Json;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "json")]
	public class JsonModule
	{
		[MoonSharpModuleMethod]
		public static DynValue parse(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			try
			{
				DynValue dynValue = args.AsType(0, "parse", DataType.String);
				Table table = JsonTableConverter.JsonToTable(dynValue.String, executionContext.GetScript());
				return DynValue.NewTable(table);
			}
			catch (SyntaxErrorException ex)
			{
				throw new ScriptRuntimeException(ex);
			}
		}

		[MoonSharpModuleMethod]
		public static DynValue serialize(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			try
			{
				DynValue dynValue = args.AsType(0, "serialize", DataType.Table);
				string str = dynValue.Table.TableToJson();
				return DynValue.NewString(str);
			}
			catch (SyntaxErrorException ex)
			{
				throw new ScriptRuntimeException(ex);
			}
		}

		[MoonSharpModuleMethod]
		public static DynValue isnull(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue dynValue = args[0];
			return DynValue.NewBoolean(JsonNull.IsJsonNull(dynValue) || dynValue.IsNil());
		}

		[MoonSharpModuleMethod]
		public static DynValue @null(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return JsonNull.Create();
		}
	}
}

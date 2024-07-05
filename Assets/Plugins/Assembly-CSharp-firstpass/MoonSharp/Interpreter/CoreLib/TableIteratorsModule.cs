namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public class TableIteratorsModule
	{
		[MoonSharpModuleMethod]
		public static DynValue ipairs(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue dynValue = args[0];
			DynValue metamethodTailCall = executionContext.GetMetamethodTailCall(dynValue, "__ipairs", args.GetArray());
			return metamethodTailCall ?? DynValue.NewTuple(DynValue.NewCallback(__next_i), dynValue, DynValue.NewNumber(0.0));
		}

		[MoonSharpModuleMethod]
		public static DynValue pairs(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue dynValue = args[0];
			DynValue metamethodTailCall = executionContext.GetMetamethodTailCall(dynValue, "__pairs", args.GetArray());
			return metamethodTailCall ?? DynValue.NewTuple(DynValue.NewCallback(next), dynValue);
		}

		[MoonSharpModuleMethod]
		public static DynValue next(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue dynValue = args.AsType(0, "next", DataType.Table);
			DynValue v = args[1];
			TablePair? tablePair = dynValue.Table.NextKey(v);
			if (tablePair.HasValue)
			{
				return DynValue.NewTuple(tablePair.Value.Key, tablePair.Value.Value);
			}
			throw new ScriptRuntimeException("invalid key to 'next'");
		}

		public static DynValue __next_i(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue dynValue = args.AsType(0, "!!next_i!!", DataType.Table);
			DynValue dynValue2 = args.AsType(1, "!!next_i!!", DataType.Number);
			int num = (int)dynValue2.Number + 1;
			DynValue dynValue3 = dynValue.Table.Get(num);
			if (dynValue3.Type != 0)
			{
				return DynValue.NewTuple(DynValue.NewNumber(num), dynValue3);
			}
			return DynValue.Nil;
		}
	}
}

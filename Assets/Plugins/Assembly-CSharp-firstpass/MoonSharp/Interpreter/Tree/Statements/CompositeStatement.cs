using System.Collections.Generic;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
	internal class CompositeStatement : Statement
	{
		private List<Statement> m_Statements = new List<Statement>();

		public CompositeStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			bool forceLast;
			do
			{
				Token current = lcontext.Lexer.Current;
				if (current.IsEndOfBlock())
				{
					break;
				}
				Statement item = Statement.CreateStatement(lcontext, out forceLast);
				m_Statements.Add(item);
			}
			while (!forceLast);
			while (lcontext.Lexer.Current.Type == TokenType.SemiColon)
			{
				lcontext.Lexer.Next();
			}
		}

		public override void Compile(ByteCode bc)
		{
			if (m_Statements == null)
			{
				return;
			}
			foreach (Statement statement in m_Statements)
			{
				statement.Compile(bc);
			}
		}
	}
}

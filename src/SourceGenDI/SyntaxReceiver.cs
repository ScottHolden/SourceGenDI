using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenDI
{
	internal class SyntaxReceiver : ISyntaxReceiver
	{
		public List<InvocationExpressionSyntax> CandidateFields { get; } = new List<InvocationExpressionSyntax>();
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is InvocationExpressionSyntax invocation)
			{
				CandidateFields.Add(invocation);
			}
		}
	}
}

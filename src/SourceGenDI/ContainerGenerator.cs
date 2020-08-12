using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenDI
{
	[Generator]
	public class ContainerGenerator : ISourceGenerator
	{
		private const string ContainerNamespace = "SourceGenDI";
		private const string ContainerTypeName = "Container";
		private const string ContainerResolveMethodName = "Resolve";
		private const string FileName = "SourceGenDI.Container.cs";

		public void Initialize(InitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(SourceGeneratorContext context)
		{
			try
			{
				string resolverFile = BuildResolver(context);

				context.AddSource(FileName, SourceText.From(resolverFile, Encoding.UTF8));

				//File.AppendAllText("C:\\temp\\builder.txt", "\n\n----\n\n" + resolverFile);
			}
			catch (Exception e)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						new DiagnosticDescriptor("D00GF00D", "ContainerError", "Error when generating container: {0}", "ContainerGenerator", DiagnosticSeverity.Error, true),
						Location.Create(FileName, new TextSpan(), new LinePositionSpan()),
						e.Message));
			}
		}

		private static string BuildResolver(SourceGeneratorContext context)
		{
			string resolverFile = BuildResolverFile(new List<ContainerService>());

			if (context.SyntaxReceiver is SyntaxReceiver receiver)
			{
				CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
				Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(resolverFile, Encoding.UTF8), options));

				List<string> typesToResolve = GetTypesToResolve(receiver, compilation);
				ServiceImplementationList si = GatherImplementations(compilation);
				List<ContainerService> services = BuildOrderedTypeList(typesToResolve, si);

				resolverFile = BuildResolverFile(services);
			}

			return resolverFile;
		}

		private static string BuildResolverFile(List<ContainerService> services)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("using System;");
			sb.AppendLine($"namespace {ContainerNamespace}");
			sb.AppendLine("{");
			sb.AppendLine($"	public sealed class {ContainerTypeName}");
			sb.AppendLine("		{");

			foreach (ContainerService service in services ?? Enumerable.Empty<ContainerService>())
			{
				sb.AppendLine($"	private static readonly {service.Type} {service.Name} = {service.InitilizerCode};");
			}

			sb.AppendLine("			public static T Resolve<T>() where T : class => (T)(typeof(T) switch {");

			foreach (ContainerService service in services ?? Enumerable.Empty<ContainerService>())
			{
				sb.AppendLine($"			Type t when t == typeof({service.Type}) => (object){service.Name},");
			}

			sb.AppendLine(@"			_ => throw new Exception(""Source generator did not emit this type?!"")");
			sb.AppendLine("			});");
			sb.AppendLine("		}");
			sb.AppendLine("}");

			return sb.ToString();
		}

		private static List<string> GetTypesToResolve(SyntaxReceiver receiver, Compilation compilation)
		{
			List<string> types = new List<string>();

			foreach (InvocationExpressionSyntax field in receiver.InvocationsToPoke)
			{
				SymbolInfo symbolInfo = compilation.GetSemanticModel(field.SyntaxTree).GetSymbolInfo(field);

				if (symbolInfo.Symbol.Kind == SymbolKind.Method &&
					symbolInfo.Symbol.ContainingNamespace.Name.Equals(ContainerNamespace) &&
					symbolInfo.Symbol.ContainingType.Name.Equals(ContainerTypeName) &&
					symbolInfo.Symbol.Name.Equals(ContainerResolveMethodName) &&
					symbolInfo.Symbol.IsStatic &&
					symbolInfo.Symbol is IMethodSymbol methodSymbol)
				{
					types.Add(methodSymbol.ReturnType.ToString());
				}
			}

			return types;
		}

		private static ServiceImplementationList GatherImplementations(Compilation compilation)
		{
			ServiceImplementationList si = new ServiceImplementationList();

			Queue<INamespaceSymbol> namespacesToProcess = new Queue<INamespaceSymbol>();
			namespacesToProcess.Enqueue(compilation.GlobalNamespace);

			while (namespacesToProcess.Count > 0)
			{
				INamespaceSymbol ns = namespacesToProcess.Dequeue();
				foreach (INamespaceSymbol cns in ns.GetNamespaceMembers())
				{
					namespacesToProcess.Enqueue(cns);
				}
				foreach (INamedTypeSymbol currentType in ns.GetTypeMembers())
				{
					string currentTypeName = currentType.ToString();
					if (!currentType.IsAbstract)
					{
						if (si.ContainsConstructor(currentTypeName) ||
							currentType.Constructors.Length > 1)
						{
							continue;
						}

						if (currentType.Constructors.Length < 1 ||
							!currentType.Constructors[0].Parameters.Any())
						{
							si.AddDefaultConstructor(currentTypeName);
						}
						else
						{
							si.AddConstructor(currentTypeName, currentType.Constructors[0].Parameters.Select(x => x.ToString()).ToList());
						}

						si.AddImplementor(currentTypeName, currentTypeName);
					}
					foreach (INamedTypeSymbol interfaceType in currentType.AllInterfaces)
					{
						si.AddImplementor(interfaceType.ToString(), currentTypeName);
					}
				}
			}

			return si;
		}
		private static List<ContainerService> BuildOrderedTypeList(List<string> initialTypes, ServiceImplementationList si)
		{
			Queue<string> toBuild = new Queue<string>(initialTypes);
			Dictionary<string, ContainerService> builder = new Dictionary<string, ContainerService>();

			while (toBuild.Count > 0)
			{
				string symbol = toBuild.Dequeue();

				if (builder.ContainsKey(symbol))
				{
					continue;
				}

				si.AddName(symbol);

				string impSymbol = si.GetImplementations(symbol).FirstOrDefault();

				List<string> ctorSymbols = si.GetConstructorParams(impSymbol);

				foreach (string cst in ctorSymbols)
				{
					si.AddName(cst);
					toBuild.Enqueue(cst);
				}

				builder.Add(symbol, new ContainerService
				{
					Type = symbol,
					Name = si.GetName(symbol),
					InitilizerCode = $"new {impSymbol}({string.Join(", ", ctorSymbols.Select(x => si.GetName(x)))})",
					Resolvable = initialTypes.Contains(symbol)
				});
			}

			return builder.Values.Reverse().ToList();
		}
	}
}

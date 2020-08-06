using Example.SameProject;
using Example.Split.Interfaces;

namespace Example.ConsoleApp
{
	public class InitalAssemblyExample : IInitalAssemblyExample
	{
		private readonly ISplitExample _splitExample;
		private readonly ITogetherExample _togetherExample;
		public InitalAssemblyExample(ISplitExample splitExample, ITogetherExample togetherExample)
		{
			_splitExample = splitExample;
			_togetherExample = togetherExample;
		}
		public int GetValue() => _splitExample.GetValue().Length + _togetherExample.GetValue().Length;
	}
}

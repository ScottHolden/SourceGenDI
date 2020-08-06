namespace Example.ConsoleApp
{
	public class Example
	{
		public Example(IInitalAssemblyExample initalAssemblyExample)
		{
			Value = initalAssemblyExample.GetValue();
		}
		public int Value { get; }
	}
}

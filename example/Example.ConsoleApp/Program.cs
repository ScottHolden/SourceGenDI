using System;

namespace Example.ConsoleApp
{
	public class Program
	{
		static void Main()
		{
			Example j = SourceGenDI.Container.Resolve<Example>();

			Console.WriteLine(j.Value);
		}
	}
}

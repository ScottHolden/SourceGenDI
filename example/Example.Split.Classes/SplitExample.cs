using System;
using Example.Split.Interfaces;

namespace Example.Split.Classes
{
	public class SplitExample : ISplitExample
	{
		private readonly IChild1 _child1;

		public SplitExample(IChild1 child1)
		{
			_child1 = child1;
		}
		public string GetValue() => _child1.GetThing();
	}

	public class Child1 : IChild1
	{
		private readonly IChild2 _child2;

		public Child1(IChild2 child2)
		{
			_child2 = child2;
		}
		public string GetThing() => _child2.GetThing();
	}
	public class Child2 : IChild2
	{
		private readonly IChild3a _child3a;
		private readonly IChild3b _child3b;

		public Child2(IChild3a child3a, IChild3b child3b)
		{
			_child3a = child3a;
			_child3b = child3b;
		}
		public string GetThing() => _child3a.GetThingA() + _child3b.GetThingB();
	}
	public class Child3 : IChild3a, IChild3b
	{
		public string GetThingA() => "a";

		public string GetThingB() => "b";
	}
	public interface IChild1 { string GetThing(); }
	public interface IChild2 { string GetThing(); }
	public interface IChild3a { string GetThingA(); }
	public interface IChild3b { string GetThingB(); }
}

using System;
using System.Collections.Generic;

namespace SourceGenDI
{
	public class ServiceImplementationList
	{
		private readonly Dictionary<string, List<string>> _implementingTypes = new Dictionary<string, List<string>>();
		private readonly Dictionary<string, List<string>> _constructorParams = new Dictionary<string, List<string>>();
		private readonly Dictionary<string, string> _names = new Dictionary<string, string>();
		private long _counter = 0;

		public void AddConstructor(string key, List<string> ctorParams)
		{
			_constructorParams[key] = ctorParams;
		}

		public void AddDefaultConstructor(string key)
		{
			_constructorParams[key] = new List<string>();
		}

		public bool ContainsConstructor(string key) => _constructorParams.ContainsKey(key);

		public void AddImplementor(string abstractType, string implementingType)
		{
			if (!_implementingTypes.ContainsKey(abstractType))
			{
				_implementingTypes.Add(abstractType, new List<string>());
			}

			_implementingTypes[abstractType].Add(implementingType);
		}

		public List<string> GetConstructorParams(string key) => _constructorParams[key];

		public List<string> GetImplementations(string key) => _implementingTypes[key];

		public string GetName(string key) => _names[key];
		public void AddName(string key)
		{
			if (!_names.ContainsKey(key))
			{
				_names.Add(key, $"s_generated{_counter++}");
			}
		}
	}
}

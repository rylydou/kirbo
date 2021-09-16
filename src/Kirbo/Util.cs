using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kirbo
{
	public static class Util
	{
		public static string ToSystem(this string str)
		{
			var sb = new StringBuilder(str.Length);

			foreach (var ch in str)
			{
				if (char.IsLetter(ch))
				{
					sb.Append(char.ToLower(ch));
					continue;
				}
				if (char.IsDigit(ch))
				{
					sb.Append(ch);
					continue;
				}
				if (char.IsWhiteSpace(ch) /* || ch == '-' */)
				{
					sb.Append('-');
					continue;
				}
			}

			return sb.ToString();
		}

		public static string CleanPath(this string str) => str.Replace('\\', '/');

		public static T PickRandom<T>(this IList<T> list) => list[MainWindow.current.rng.Next(list.Count)];

		public static T?[] ReadAsJArray<T>(this JsonReader reader)
		{
			var jArray = JArray.Load(reader);
			var items = new List<T?>();

			foreach (var token in jArray)
			{
				items.Add(token.Value<T>());
			}

			return items.ToArray();
		}
	}
}

using System.Text;

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
	}
}

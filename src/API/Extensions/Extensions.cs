namespace API.Extensions
{
	public static class String_Extensions
	{
		public static string ToPhpCompatibleHexString(this byte[] value)
		{
			return string.Join("", value.Select(b => b.ToString("x2")));
		}
	}
}

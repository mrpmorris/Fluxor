using System;
using System.Security.Cryptography;
using System.Text;

namespace Fluxor.StoreBuilderSourceGenerator;

internal static class FilenameGenerator
{
	public static string Generate(string classNamespace, string className)
	{
		using SHA256 sha256Hash = SHA256.Create();
		byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes($"{classNamespace}/{className}"));
		string result = $"{className}--{Convert.ToBase64String(bytes)}"
			.Replace('/', '-');
		return result.Substring(0, result.Length - 1);
	}
}

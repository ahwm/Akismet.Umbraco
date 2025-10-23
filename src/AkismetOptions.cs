namespace Akismet.Umbraco
{
	public class AkismetOptions
	{
		public const string SectionName = "Akismet";

		/// <summary>
		/// The Akismet API key for spam detection
		/// </summary>
		public string ApiKey { get; set; } = string.Empty;
	}
}
namespace TGramWeb.Services.GitlabProcessService
{
    internal static class StringExtensions
    {
        public static string Md(this string str)
        {
            return !string.IsNullOrEmpty(str)
                       ? str.Replace("[", "").Replace("]", "")
                       : str;
        }
    }
}

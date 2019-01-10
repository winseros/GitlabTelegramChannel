namespace TGramWeb.Services.GitlabProcessService
{
    internal static class StringExtensions
    {
        public static string MarkdownEscape(this string str)
        {
            return !string.IsNullOrEmpty(str)
                       ? str.Replace("[", "")
                            .Replace("]", "")
                            .Replace("(", "\\(")
                            .Replace(")", "\\)")
                       : str;
        }
    }
}

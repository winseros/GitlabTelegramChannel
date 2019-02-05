using TGramWeb.Services.GitlabProcessService;
using Xunit;

namespace TGramWeb.Test.Services.GitlabProcessService
{
    public class DateComparerTest
    {
        [Theory]
        [InlineData(null, null, true)]
        [InlineData("", "", true)]
        [InlineData("", "abc", false)]
        [InlineData("abc", null, false)]
        [InlineData("abc", "abc", true)]
        [InlineData("2019-02-04 09:21:26 UTC", "2019-02-04 09:21:27 UTC", true)]
        [InlineData("2018-05-15T05:10:20+0400", "2018-05-15T05:10:20+0400", true)]
        [InlineData("2018-05-15T05:10:20+0400", "2018-05-15T05:10:22+0400", true)]
        [InlineData("2018-05-15T05:10:22+0400", "2018-05-15T05:10:20+0400", true)]
        [InlineData("2018-05-15T06:10:20+0400", "2018-05-15T05:10:20+0400", false)]
        [InlineData("2018-05-15T04:10:20+0400", "2018-05-15T05:10:20+0400", false)]
        [InlineData("123", "456", false)]
        public void Test_DateStringsMatch_Functional(string date1, string date2, bool expectedResult)
        {
            bool result = DateComparer.DateStringsMatch(date1, date2);
            Assert.Equal(expectedResult, result);
        }
    }
}

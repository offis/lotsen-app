using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Rest.Test
{
    [ExcludeFromCodeCoverage]
    public class LanguageDtoTest
    {
        [Fact]
        public void ShouldSetLanguage()
        {
            var dto = new LanguageDto();
            const string language = "latin";
            dto.Language = language;
            
            Assert.Equal(language, dto.Language);
        }
    }
}
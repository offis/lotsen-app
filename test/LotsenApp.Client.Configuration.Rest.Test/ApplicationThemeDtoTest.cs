using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Rest.Test
{
    [ExcludeFromCodeCoverage]
    public class ApplicationThemeDtoTest
    {
        [Fact]
        public void ShouldSetApplicationTheme()
        {
            var dto = new ApplicationThemeDto();
            const string theme = "dark-theme";
            dto.Theme = theme;
            Assert.Equal(theme, dto.Theme);
            
        }
    }
}
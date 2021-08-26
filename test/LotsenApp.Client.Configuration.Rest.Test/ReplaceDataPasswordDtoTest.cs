using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Rest.Test
{
    [ExcludeFromCodeCoverage]
    public class ReplaceDataPasswordDtoTest
    {
        [Fact]
        public void ShouldSetDataPassword()
        {
            var dto = new ReplaceDataPasswordDto();
            var pw = Guid.NewGuid().ToString();
            dto.NewDataPassword = pw;
            
            Assert.Equal(pw, dto.NewDataPassword);
        }
        
        [Fact]
        public void ShouldSetRecoveryKey()
        {
            var dto = new ReplaceDataPasswordDto();
            var pw = Guid.NewGuid().ToString();
            dto.RecoveryKey = pw;
            
            Assert.Equal(pw, dto.RecoveryKey);
        }
        
    }
}
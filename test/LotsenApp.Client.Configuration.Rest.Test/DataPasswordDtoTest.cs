using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Rest.Test
{
    [ExcludeFromCodeCoverage]
    public class DataPasswordDtoTest
    {
        [Fact]
        public void ShouldSetDataPassword()
        {
            var dto = new DataPasswordDto();
            var pw = Guid.NewGuid().ToString();
            dto.DataPassword = pw;
            
            Assert.Equal(pw, dto.DataPassword);
        }
        
        [Fact]
        public void ShouldSetRecoveryKey()
        {
            var dto = new DataPasswordDto();
            var pw = Guid.NewGuid().ToString();
            dto.RecoveryKey = pw;
            
            Assert.Equal(pw, dto.RecoveryKey);
        }
        
        [Fact]
        public void ShouldHaveInitialEnforceUpdate()
        {
            var dto = new DataPasswordDto();
            
            Assert.False(dto.EnforceUpdate);
        }
        
        [Fact]
        public void ShouldSetEnforceUpdate()
        {
            var dto = new DataPasswordDto
            {
                EnforceUpdate = true
            };
            Assert.True(dto.EnforceUpdate);
        }
    }
}
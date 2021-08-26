using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public class ProgrammeDefinitionTest
    {
        [Fact]
        public void ShouldSetLabel()
        {
            var definition = new ProgrammeDefinition();
            var label = Guid.NewGuid().ToString();
            definition.Label = label;
            
            Assert.Equal(label, definition.Label);
        } 
        
        [Fact]
        public void ShouldSetPath()
        {
            var definition = new ProgrammeDefinition();
            var path = Guid.NewGuid().ToString();
            definition.Path = path;
            
            Assert.Equal(path, definition.Path);
        } 
    }
}
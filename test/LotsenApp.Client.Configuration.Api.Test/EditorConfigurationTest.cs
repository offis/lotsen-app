using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public class EditorConfigurationTest
    {
        [Fact]
        public void ShouldSetOpenedDocuments()
        {
            var editorConfiguration = new EditorConfiguration();

            var newValue = new Dictionary<string, string[]>
            {
                {Guid.NewGuid().ToString(), new []
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }},
                {Guid.NewGuid().ToString(), new []
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }},
                {Guid.NewGuid().ToString(), new []
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }},
                {Guid.NewGuid().ToString(), new []
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }},
            };
            editorConfiguration.OpenedDocuments = newValue;
            
            Assert.Same(newValue, editorConfiguration.OpenedDocuments);
        }
    }
}
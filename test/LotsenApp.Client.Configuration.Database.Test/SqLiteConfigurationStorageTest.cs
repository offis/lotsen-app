using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace LotsenApp.Client.Configuration.Database.Test
{
    [ExcludeFromCodeCoverage]
    public class SqLiteConfigurationStorageTest: DatabaseConfigurationStorageTest
    {
        public SqLiteConfigurationStorageTest() : 
            base(new DbContextOptionsBuilder<DatabaseConfigurationContext>()
                .UseSqlite("Filename=Test.db")
                .Options)
        {
        }
    }
}
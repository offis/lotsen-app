using System.Threading.Tasks;

namespace LotsenApp.Development.Setup
{
    public interface ISetupProvider
    {
        public Task PerformSetup();
    }
}
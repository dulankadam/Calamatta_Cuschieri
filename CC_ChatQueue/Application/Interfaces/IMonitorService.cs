using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMonitorService
    {
        Task RunChecksAsync();
    }
}

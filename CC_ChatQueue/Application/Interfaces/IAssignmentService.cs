using Domain.Models;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAssignmentService
    {
        Task AssignNextAsync();
    }
}

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Interfaces
{

    public interface ICommand<TResult> : IAction<TResult>
    {
    }

    
}

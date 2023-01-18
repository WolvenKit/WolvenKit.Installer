using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolvenkit.Installer.Services;
public interface IDialogService
{
    Task DisplayAlert(string title, string message, string cancel);
}

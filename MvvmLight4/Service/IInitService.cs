using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Service
{
    public interface IInitService
    {
        int InitDatabase(string sqlFile);
    }
}

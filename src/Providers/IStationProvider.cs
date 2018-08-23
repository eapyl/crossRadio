using System.Collections.Generic;
using System.Threading.Tasks;
using plr.Entities;

namespace plr.Providers
{
    internal interface IStationProvider
    {
        Task<Station> Current();
        void Reset();
        Task<Station> Search(int id);
        Task<IEnumerable<Station>> Search(string name = null);
    }
}
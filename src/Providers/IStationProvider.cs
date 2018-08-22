using System.Collections.Generic;
using System.Threading.Tasks;
using plr.Entities;

namespace plr.Providers
{
    internal interface IStationProvider
    {
        Task LoadStation();
        Station Current { get; }
        Station Search(int id);
        IList<Station> Search(string name = null);
    }
}
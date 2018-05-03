using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;

namespace rsRadio
{
    internal class StationManager
    {
        private string _databaseLink = string.Empty;

        private IList<Station> _stations = new List<Station>();

        public IList<Station> Search(string name = null) =>
            _stations.Where(x => name == null ? true : x.Name.Contains(name)).ToList();

        public Station Current {get { return _stations.Any() ? _stations.First() : null; } }

        public StationManager(string databaseLink)
        {
            _databaseLink = databaseLink;
        }

        public async Task LoadStation()
        {
            _stations = await _databaseLink.GetJsonAsync<List<Station>>();
        }
    }
}
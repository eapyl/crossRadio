using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;

namespace plr
{
    internal class StationManager
    {
        private Settings _settings { get; }

        private IList<Station> _stations = new List<Station>();

        public IList<Station> Search(string name = null) =>
            _stations.Where(x => name == null ? true : x.Name.Contains(name)).ToList();

        public Station Search(int id) =>
            _stations.Where(x => x.Id == id).FirstOrDefault();

        public Station Current {get { return _stations.Any() ? _stations.First() : null; } }

        public StationManager(Settings settings)
        {
            _settings = settings;
        }

        public async Task LoadStation()
        {
            _stations = await _settings.DatabaseLink.GetJsonAsync<List<Station>>();
            var i = 0;
            foreach (var st in _stations)
                st.Id = i++;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using plr.Entities;
using Serilog;

namespace plr.Providers
{
    internal class StationProvider : IStationProvider
    {
        private IList<Station> _stations = new List<Station>();
        private readonly ILogger _log;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly StationValidator _stationValidator;

        public IList<Station> Search(string name = null) =>
            _stations.Where(x => name == null ? true : x.Name.Contains(name)).ToList();

        public Station Search(int id) =>
            _stations.Where(x => x.Id == id).FirstOrDefault();

        public Station Current {get { return _stations.Any() ? _stations.First() : null; } }

        public StationProvider(
            ILogger log,
            IConfigurationProvider configurationProvider,
            StationValidator stationValidator)
        {
            _log = log;
            _configurationProvider = configurationProvider;
            _stationValidator = stationValidator;
        }

        public async Task LoadStation()
        {
            _log.Verbose("Load station from database");
            var configuration = await _configurationProvider.Load();
            _stations = await configuration.DatabaseLink.GetJsonAsync<List<Station>>();
            var i = 0;
            _stations = _stations.Where(x => {
                var result = _stationValidator.Validate(x);
                if (!result.IsValid)
                {
                    foreach(var failure in result.Errors)
                    {
                        _log.Error("Property " + failure.PropertyName + " failed validation. Error was: " + failure.ErrorMessage);
                    }
                }
                return result.IsValid;
            }).ToList();

            foreach (var st in _stations)
            {
                st.Id = i++;
            }
        }
    }
}
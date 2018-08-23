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
        private IEnumerable<Station> _stations;
        private readonly ILogger _log;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly StationValidator _stationValidator;

        public async Task<IEnumerable<Station>> Search(string name = null) =>
            (await GetStations()).Where(x => name == null ? true : x.Name.Contains(name));

        public async Task<Station> Search(int id) =>
            (await GetStations()).Where(x => x.Id == id).FirstOrDefault();

        public async Task<Station> Current() => (await GetStations()).Any() ? (await GetStations()).First() : null;

        public StationProvider(
            ILogger log,
            IConfigurationProvider configurationProvider,
            StationValidator stationValidator)
        {
            _log = log;
            _configurationProvider = configurationProvider;
            _stationValidator = stationValidator;
        }

        internal async Task<IEnumerable<Station>> GetStations()
        {
            // TODO: multithreading ??
            if (_stations == null)
                _stations = await LoadStation();
            return _stations;
        }

        public async Task<IEnumerable<Station>> LoadStation()
        {
            _log.Verbose("Load station from database");
            var configuration = await _configurationProvider.Load();
            var i = 0;
            return (await configuration.DatabaseLink.GetJsonAsync<List<Station>>()).Where(x =>
            {
                var result = _stationValidator.Validate(x);
                if (!result.IsValid)
                {
                    foreach(var failure in result.Errors)
                    {
                        _log.Error("Property " + failure.PropertyName + " failed validation. Error was: " + failure.ErrorMessage);
                    }
                }
                return result.IsValid;
            }).Select(st =>
            {
                st.Id = i++;
                return st;
            });
        }

        public void Reset() => _stations = null;
    }
}
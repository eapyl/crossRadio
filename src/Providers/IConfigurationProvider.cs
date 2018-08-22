using System.Threading.Tasks;
using plr.Entities;

namespace plr.Providers
{
    internal interface IConfigurationProvider
    {
        Task<Configuration> Load();
        Task Upload(Configuration settings);
    }
}
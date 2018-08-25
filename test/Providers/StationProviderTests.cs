using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Flurl.Http.Testing;
using plr;
using plr.Commands;
using plr.Entities;
using plr.Providers;
using Serilog;
using Xunit;

namespace test.Providers
{
    public class StationProviderTests
    {
        [Fact]
        public void Create()
        {
            var logger = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();

            var provider = new StationProvider(logger, configurationProvider, new StationValidator());
            Assert.NotNull(provider);
        }

        [Fact]
        public async Task SearchAll()
        {
            var logger = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();

            A.CallTo(() => configurationProvider.Load()).Returns(new Configuration {DatabaseLink = "http://test"});

            var provider = new StationProvider(logger, configurationProvider, new StationValidator());
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new[] { new {
                    Name = "Station",
                    Uri = new [] { "http:\\\\test" },
                    Country = "Station",
                    Language = new [] { "Station" }
                }});
                var result = await provider.Search();
                Assert.Contains(result, x => x.Name == "Station");
                Assert.Contains(result, x => x.Country == "Station");
                Assert.Contains(result, x => x.Uri.Contains("http:\\\\test"));
                Assert.Contains(result, x => x.Language.Contains("Station"));
            }
        }

        [Fact]
        public async Task SearchById()
        {
            var logger = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();

            A.CallTo(() => configurationProvider.Load()).Returns(new Configuration {DatabaseLink = "http://test"});

            var provider = new StationProvider(logger, configurationProvider, new StationValidator());
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new[] { new {
                    Name = "Station",
                    Uri = new [] { "http:\\\\test" },
                    Country = "Station",
                    Language = new [] { "Station" }
                }});
                var result = await provider.Search(0);
                Assert.Equal("Station", result.Name);
                Assert.Equal("Station", result.Country);
                Assert.Contains(result.Uri, x => x == "http:\\\\test");
                Assert.Contains(result.Language, x => x == "Station");
            }
        }

        [Fact]
        public async Task Current()
        {
            var logger = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();

            A.CallTo(() => configurationProvider.Load()).Returns(new Configuration {DatabaseLink = "http://test"});

            var provider = new StationProvider(logger, configurationProvider, new StationValidator());
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new[] { new {
                    Name = "Station",
                    Uri = new [] { "http:\\\\test" },
                    Country = "Station",
                    Language = new [] { "Station" }
                }});
                var result = await provider.Current();
                Assert.Equal("Station", result.Name);
                Assert.Equal("Station", result.Country);
                Assert.Contains(result.Uri, x => x == "http:\\\\test");
                Assert.Contains(result.Language, x => x == "Station");
            }
        }

        [Fact]
        public async Task IncorrecrStation()
        {
            var logger = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();

            A.CallTo(() => configurationProvider.Load()).Returns(new Configuration {DatabaseLink = "http://test"});

            var provider = new StationProvider(logger, configurationProvider, new StationValidator());
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new[] { new {
                    Name = "",
                    Uri = new [] { "http:\\\\test" },
                    Country = "Station",
                    Language = new [] { "Station" }
                }});
                var result = await provider.Search();
                Assert.True(!result.Any());
            }
            A.CallTo(() => logger.Error(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public async Task StationShouldBeSavedInMemory()
        {
            var logger = A.Fake<ILogger>();
            var configurationProvider = A.Fake<IConfigurationProvider>();

            A.CallTo(() => configurationProvider.Load()).Returns(new Configuration {DatabaseLink = "http://test"});

            var provider = new StationProvider(logger, configurationProvider, new StationValidator());
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new[] { new {
                    Name = "Station",
                    Uri = new [] { "http:\\\\test" },
                    Country = "Station",
                    Language = new [] { "Station" }
                }});
                var result = await provider.Search();
                Assert.Contains(result, x => x.Id == 0);

                result = await provider.Search();
                Assert.Contains(result, x => x.Id == 0);
            }
        }
    }
}
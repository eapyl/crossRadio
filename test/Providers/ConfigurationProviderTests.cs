using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using plr;
using plr.Commands;
using plr.Entities;
using plr.Providers;
using Serilog;
using Xunit;

namespace test.Providers
{
    public class ConfigurationProviderTests
    {
        [Fact]
        public void Create()
        {
            var fileContent = A.Fake<Func<string, Task<string>>>();
            var saveContent = A.Fake<Func<string, string, Task>>();

            var provider = new ConfigurationProvider(string.Empty, fileContent, saveContent);
            Assert.NotNull(provider);
        }

        [Fact]
        public async Task Load()
        {
            var fileContent = A.Fake<Func<string, Task<string>>>();
            var saveContent = A.Fake<Func<string, string, Task>>();

            A.CallTo(() => fileContent(A<string>._)).Returns("{\"databaseLink\":\"link\"}");

            var provider = new ConfigurationProvider(string.Empty, fileContent, saveContent);

            var configuration = await provider.Load();
            Assert.Equal("link", configuration.DatabaseLink);
        }

        [Fact]
        public async Task Upload()
        {
            var fileContent = A.Fake<Func<string, Task<string>>>();
            var saveContent = A.Fake<Func<string, string, Task>>();

            var provider = new ConfigurationProvider(string.Empty, fileContent, saveContent);

            await provider.Upload(new Configuration());
            A.CallTo(() => saveContent(A<string>._, A<string>._)).MustHaveHappened();
        }
    }
}
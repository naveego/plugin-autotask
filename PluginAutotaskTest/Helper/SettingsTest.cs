using System;
using PluginAutotask.Helper;
using Xunit;

namespace PluginAutotaskTest.Helper
{
    public class SettingsTest
    {
        [Fact]
        public void ValidateValidTest()
        {
            // setup
            var settings = new Settings
            {
                Secret = "SECRET",
                ApiZone = "ZONE",
                UserName = "USERNAME",
                ApiIntegrationCode = "IDCODE",
            };

            // act
            settings.Validate();

            // assert
        }

        [Fact]
        public void ValidateNoUsernameTest()
        {
            // setup
            var settings = new Settings
            {
                Secret = "SECRET",
                ApiZone = "ZONE",
                UserName = null,
                ApiIntegrationCode = "IDCODE",
            };

            // act
            Exception e = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the UserName property must be set", e.Message);
        }
        [Fact]
        public void ValidateNoSecretTest()
        {
            // setup
            var settings = new Settings
            {
                Secret = null,
                ApiZone = "ZONE",
                UserName = "USERNAME",
                ApiIntegrationCode = "IDCODE",
            };

            // act
            Exception e = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the Secret property must be set", e.Message);
        }
        [Fact]
        public void ValidateNoApiZoneTest()
        {
            // setup
            var settings = new Settings
            {
                Secret = "SECRET",
                ApiZone = null,
                UserName = "USERNAME",
                ApiIntegrationCode = "IDCODE",
            };

            // act
            Exception e = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the ApiZone property must be set", e.Message);
        }
        [Fact]
        public void ValidateNoApiIntegrationCodeTest()
        {
            // setup
            var settings = new Settings
            {
                Secret = "SECRET",
                ApiZone = "ZONE",
                UserName = "USERNAME",
                ApiIntegrationCode = null,
            };

            // act
            Exception e = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the ApiIntegrationCode property must be set", e.Message);
        }
    }
}
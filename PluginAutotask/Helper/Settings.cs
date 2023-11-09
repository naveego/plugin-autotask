using System;

namespace PluginAutotask.Helper
{
    public class Settings
    {
        public string UserName { get; set; }
        public string Secret { get; set; }
        public string ApiIntegrationCode { get; set; }
        public string ApiZone { get; set; }
        public int ApiUsageThreshold {get; set; }
        public int ApiDelayIntervalSeconds {get; set; }

        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            if (String.IsNullOrEmpty(UserName))
            {
                throw new Exception("The UserName property must be set");
            }
            if (String.IsNullOrEmpty(Secret))
            {
                throw new Exception("The Secret property must be set");
            }
            if (String.IsNullOrEmpty(ApiIntegrationCode))
            {
                throw new Exception("The ApiIntegrationCode property must be set");
            }
            if (String.IsNullOrEmpty(ApiZone))
            {
                throw new Exception("The ApiZone property must be set");
            }
            if (ApiUsageThreshold == null || ApiUsageThreshold <= 0 || ApiUsageThreshold >= 10000)
            {
                throw new Exception("The ApiDelayThreshold property must be set to a non-zero positive number less than 10,000");
            }
            if (ApiDelayIntervalSeconds == null || ApiDelayIntervalSeconds <= 0)
            {
                throw new Exception("The ApiDelayIntervalSeconds property must be set to a non-zero positive number");
            }
        }
    }
}
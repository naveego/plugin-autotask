using System;

namespace PluginHubspot.Helper
{
    public class Settings
    {
        public string UserName { get; set; }
        public string Secret { get; set; }
        public string ApiIntegrationCode { get; set; }
        public string ApiZone { get; set; }

        
        
        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            
            if (String.IsNullOrEmpty(UserName))
            {
                throw new Exception("the UserName property must be set");
            }
            if (String.IsNullOrEmpty(Secret))
            {
                throw new Exception("the Secret property must be set");
            }
            if (String.IsNullOrEmpty(ApiIntegrationCode))
            {
                throw new Exception("the ApiIntegrationCode property must be set");
            }
            if (String.IsNullOrEmpty(ApiZone))
            {
                throw new Exception("the ApiZone property must be set");
            }
        }
    }
}
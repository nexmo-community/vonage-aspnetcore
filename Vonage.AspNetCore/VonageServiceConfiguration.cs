using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Vonage.Cryptography;
using Vonage.Request;

namespace Vonage.AspNetCore
{
    /// <summary>
    /// Extension class for voange service configuration;
    /// </summary>
    public static class  VonageServiceConfiguration
    {
        /// <summary>
        /// Allows dependency injection of a singleton <see cref="VonageClient"/> based off of the configuration of your app
        /// Should be invoked from the <code>ConfigureServices</code> method of your startup file. Keys expected in the configuration object include:
        /// <code>Vonage:API_KEY</code> - your API Key
        /// <code>Vonage:API_SECRET</code> - your API secret
        /// <code>Vonage:PRIVATE_KEY_PATH</code> - your Application's private key's path
        /// <code>Vonage:APPLICATION_ID</code> - your applicaiton id
        /// <code>Vonage:SIGNATURE_SECRET</code> - your signature secret
        /// <code></code>
        /// if those keys are not present in your configuration the VonageClient will still initalize and be injectable to your other services. 
        /// However you will need to provide crednentials whenever invoking one of the VonageClient's requests.        
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddVonageService(this IServiceCollection services, IConfiguration configuration)
        {            
            var apiKey = configuration["Vonage:API_KEY"];
            var apiSecret = configuration["Vonage:API_SECRET"];
            var privateKeyPath = configuration["Vonage:PRIVATE_KEY_PATH"];
            var appId = configuration["Vonage:APPLICATION_ID"];
            var signatureSecret = configuration["Vonage:SIGNATURE_SECRET"];
            SmsSignatureGenerator.Method signatureSecretMethod; 
            var methodProvided = Enum.TryParse(configuration["Vonage:SIGNATURE_METHOD"], out signatureSecretMethod);
            Credentials creds = null;

            // if a private key path and an app id are provided initalize the credentials object by reading the private key out of the provide file
            // initalize any other parameters that we find
            if(!string.IsNullOrEmpty(privateKeyPath) && !string.IsNullOrEmpty(appId))
            {
                creds = Credentials.FromAppIdAndPrivateKeyPath(appId, privateKeyPath);
                creds.ApiKey = apiKey;
                creds.ApiSecret = apiSecret;
                if (methodProvided)
                {
                    creds.Method = signatureSecretMethod;
                    creds.ApiKey = apiKey;
                    creds.SecuritySecret = signatureSecret;
                }
            }
            // if an api key and secret are provided, initalize from api key and secret - provide signature auth if possible
            else if(!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
            {
                creds = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
                if(methodProvided)
                {
                    creds.Method = signatureSecretMethod;
                    creds.ApiKey = apiKey;
                    creds.SecuritySecret = signatureSecret;
                }
            }
            // if no other method is found to initalize credentials, and a signature secret, method, and key are provided initalize it here
            else if(!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(signatureSecret) && methodProvided){
                creds = Credentials.FromApiKeySignatureSecretAndMethod(apiKey, signatureSecret, signatureSecretMethod);
            }
            var client = new VonageClient(creds);
            services.AddSingleton(client);
        }

        /// <summary>
        /// Depenency injects a <see cref="VonageClient"/> object constructed from the provided <see cref="Credentials"/> object
        /// </summary>
        /// <param name="services"></param>
        /// <param name="creds"></param>
        public static void AddVonageService(this IServiceCollection services, Credentials creds) => services.AddSingleton(new VonageClient(creds));
    }
}

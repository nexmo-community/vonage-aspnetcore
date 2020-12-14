using Vonage.Messaging;
namespace Vonage.AspNetCore
{
    /// <summary>
    /// Delegate used to handle inbound SMS messages, if you pass your security secret into the middleware, this SMS will have already been pre-validated
    /// </summary>
    /// <param name="sms"></param>
    public delegate void InboundSmsDelegate(InboundSms sms);    

    /// <summary>
    /// Delegate for a generic webhook parsed from an ASP.NET route
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="webhook"></param>
    public delegate void WebhookDelegate<T>(T webhook);
}

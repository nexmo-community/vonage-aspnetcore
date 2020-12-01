using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Vonage.Cryptography;
using Vonage.Messaging;
using Vonage.Utility;

namespace Vonage.AspNetCore
{
    internal class InboundSmsWebhookHandler
    {
        private readonly RequestDelegate _next;
        private readonly InboundSmsDelegate _delegate;
        private readonly string _signatureSecret;
        private readonly SmsSignatureGenerator.Method _signatureMethod;
        private readonly bool _invokeNextMiddleware;
        internal InboundSmsWebhookHandler(RequestDelegate next, 
            InboundSmsDelegate handler,
            string signatureSecret = null, 
            SmsSignatureGenerator.Method method = SmsSignatureGenerator.Method.md5hash,
            bool invokeNext = false)
        {
            _delegate = handler;
            _next = next;
            _signatureSecret = signatureSecret;
            _signatureMethod = method;
            _invokeNextMiddleware = invokeNext;            
        }
        internal async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                InboundSms sms;
                if (httpContext.Request.Method == "GET")
                {
                    sms = WebhookParser.ParseQuery<InboundSms>(httpContext.Request.Query);                                        
                }
                else
                {
                    sms = await WebhookParser.ParseWebhookAsync<InboundSms>(httpContext.Request.Body, httpContext.Request.ContentType);                    
                }
                if(!string.IsNullOrEmpty(_signatureSecret))
                {
                    if(sms.ValidateSignature(_signatureSecret, _signatureMethod))
                    {
                        httpContext.Response.StatusCode = 204;
                        _delegate(sms);
                    }
                    else
                    {
                        httpContext.Response.StatusCode = 401;
                    }
                }
                else
                {
                    httpContext.Response.StatusCode = 204;
                    _delegate(sms);
                }
            }
            catch (Exception)
            {
                httpContext.Response.StatusCode = 500;
            }
            if (_invokeNextMiddleware)
            {
                await _next(httpContext); 
            }
        }        
    }

    /// <summary>
    /// Extension class for inbound webhooks. 
    /// </summary>
    public static class InboundSmsWebhookExtension
    {
        /// <summary>
        /// Adds a piece of middleware to handle Inbound SMS messages. Will return a 204 if it encounters a valid sms, 401 if it can't authenticate the SMS, and a 500 if anything else goes wrong
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="handler">Delegate to handle the inbound SMS</param>
        /// <param name="path">the route you'd like to handle the inbound SMS over, this should correspond to what you've set in the vonage dashboard</param>
        /// <param name="signatureSecret">The signature secret to validate the SMS Against, defaults to null, if this is not set the SMS will not be validated</param>
        /// <param name="method">Signature method to use for validating SMS messages</param>
        /// <param name="invokeNextMiddleware">Whetehr to invoke the next piece of middleware in your pipeline, if this is false this middleware will exit immediately after completing</param>
        /// <returns></returns>
        public static IApplicationBuilder UseVonageSms(this IApplicationBuilder builder, 
            InboundSmsDelegate handler, 
            string path = "/webhooks/inbound-sms", 
            string signatureSecret = null,
            SmsSignatureGenerator.Method method = SmsSignatureGenerator.Method.md5hash,
            bool invokeNextMiddleware = false)
        {            
            return builder.Map(path, b => b.UseMiddleware<InboundSmsWebhookHandler>(handler, signatureSecret, method, invokeNextMiddleware));
        }
    }
}

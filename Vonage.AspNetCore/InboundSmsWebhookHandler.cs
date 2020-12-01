using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Vonage.Messaging;
using Vonage.Utility;
using Vonage.Cryptography;

namespace Vonage.AspNetCore
{
    public class InboundSmsWebhookHandler
    {
        private readonly RequestDelegate _next;
        private readonly InboundSmsDelegate _delegate;
        private readonly string _signatureSecret;
        private readonly SmsSignatureGenerator.Method _signatureMethod;
        private readonly bool _invokeNextMiddleware;        
        public InboundSmsWebhookHandler(RequestDelegate next, 
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
        public async Task InvokeAsync(HttpContext httpContext)
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

    public static class InboundSmsWebhookExtension
    {
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

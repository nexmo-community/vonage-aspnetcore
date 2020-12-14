using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Vonage.Utility;

namespace Vonage.AspNetCore
{
    internal class WebhookMiddleware<T> where T : class
    {
        private readonly RequestDelegate _next;
        private readonly WebhookDelegate<T> _handler;
        private readonly bool _invokeNext;
        public WebhookMiddleware(RequestDelegate next, WebhookDelegate<T> handler, bool invokeNextMiddleware)
        {
            _next = next;
            _handler = handler;
            _invokeNext = invokeNextMiddleware;
        }

        internal async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                if(httpContext.Request.Method == "GET")
                {
                    var webhook = WebhookParser.ParseQuery<T>(httpContext.Request.Query);
                    _handler(webhook);
                }
                else if (httpContext.Request.Method == "POST")
                {
                    var webhook = await WebhookParser.ParseWebhookAsync<T>(httpContext.Request.Body, httpContext.Request.ContentType);
                    _handler(webhook);
                }
                httpContext.Response.StatusCode = 204;
            }
            catch (Exception)
            {
                httpContext.Response.StatusCode = 500;
            }
            if (_invokeNext)
            {
                await _next(httpContext);
            }
        }
    }

    /// <summary>
    /// Extension for handling generic Vonage webhooks
    /// </summary>
    public static class VonageWebhookMiddlewareExtension
    {
        /// <summary>
        /// Handles generic webhooks from Vonage
        /// </summary>
        /// <typeparam name="T">The type of webhook you'd like to handle, e.g. <see cref="Messaging.DeliveryReceipt"/></typeparam>
        /// <param name="builder"></param>
        /// <param name="handler">the delegate to call back on when the webhook is parsed</param>
        /// <param name="path">The path to handle the webhook on</param>
        /// <param name="invokeNextMiddleware">whether to invoke the next piece of middleware in your pipeline</param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebhookRoute<T>(this IApplicationBuilder builder,
            WebhookDelegate<T> handler,
            string path,
            bool invokeNextMiddleware = false) where T : class
        {
            return builder.Map(path, b => b.UseMiddleware<WebhookMiddleware<T>>(handler, invokeNextMiddleware));
        }
    }
}

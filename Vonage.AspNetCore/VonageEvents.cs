using System;
using System.Collections.Generic;
using System.Text;
using Vonage.Messaging;
namespace Vonage.AspNetCore
{
    public delegate void InboundSmsDelegate(InboundSms sms);    
    public delegate void WebhookDelegate<T>(T webhook);
}

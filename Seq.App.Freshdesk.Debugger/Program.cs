using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Seq.App.Freshdesk.Debugger
{
    class Program
    {
        static void Main(string[] args)
        {
            var reactor = new FreshdeskReactor();
            reactor.Identifier = "##APIKEY##";
            reactor.Subdomain = "##SUBDOMAIN##";
            reactor.RequesterEmail = "##REQUESTER_EMAIL##";
            reactor.RequesterName = "##REQUESTER_NAME##";
            reactor.TicketType = "##VALID_TICKET_TYPE##";

            reactor.Attach();

            reactor.On(new Apps.Event<Apps.LogEvents.LogEventData>("test", 1, DateTime.UtcNow, new Apps.LogEvents.LogEventData
            {
                Exception = null,
                Level = Apps.LogEvents.LogEventLevel.Fatal,
                LocalTimestamp = DateTime.Now,
                MessageTemplate = "##NOT_USED_BY_SEQ_APP##",
                RenderedMessage = "##MESSAGE##",
                Properties = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>() { { "Prop1", "My Value1" }, { "Prop2", "My Value2" } })
            }));
        }
    }
}

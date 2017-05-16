using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Seq.App.Freshdesk.Debugger
{
    class Program
    {
        static void Main(string[] args)
        {
            var reacator = new FreshdeskReactor();
            reacator.Identifier = "##APIKEY##";
            reacator.Subdomain = "##SUBDOMAIN##";
            reacator.RequesterEmail = "##REQUESTER_EMAIL##";
            reacator.RequesterName = "##REQUESTER_NAME##";
            reacator.TicketType = "##VALID_TICKET_TYPE##";

            reacator.Attach();

            reacator.On(new Apps.Event<Apps.LogEvents.LogEventData>("test", 1, DateTime.UtcNow, new Apps.LogEvents.LogEventData
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

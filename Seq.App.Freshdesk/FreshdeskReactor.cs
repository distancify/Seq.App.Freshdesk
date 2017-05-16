using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Linq;
using System.Net;

namespace Seq.App.Freshdesk
{
    [SeqApp("Freshdesk", Description = "Creates tickets in freshdesk for events.")]
    public class FreshdeskReactor : Reactor, ISubscribeTo<LogEventData>
    {
        private FreshdeskClient _client;

        [SeqAppSetting(DisplayName = "Freskdesk subdomain", HelpText = "Your freshdesk subdomain {subdomain}.freshdesk.com", IsOptional = false)]
        public string Subdomain { get; set; }

        [SeqAppSetting(DisplayName = "Freskdesk username or api key", HelpText = "Your freshdesk username or api key", IsOptional = false)]
        public string Identifier { get; set; }

        [SeqAppSetting(DisplayName = "Freshdesk password", HelpText = "Your freshdesk password (not needed when api key is provided)", InputType = SettingInputType.Password, IsOptional = true)]
        public string Password { get; set; }

        [SeqAppSetting(DisplayName = "Requester email", HelpText = "The email that will be used as requester email on the tickets", IsOptional = false)]
        public string RequesterEmail { get; set; }

        [SeqAppSetting(DisplayName = "Requester name", HelpText = "The name that will be used as requester name on the tickets", IsOptional = false)]
        public string RequesterName { get; set; }

        [SeqAppSetting(DisplayName = "Ticket type", HelpText = "The type of ticket to create in freshdesk", IsOptional = true)]
        public string TicketType { get; set; }

        [SeqAppSetting(DisplayName = "Ticket subject prefix", HelpText = "Add a prefix to your ticket subject", IsOptional = true)]
        public string TicketSubjectPrefix { get; set; } = "";

        public void On(Event<LogEventData> evt)
        {
            var ticket = new Ticket();
            ticket.Email = RequesterEmail;
            ticket.Name = RequesterName;
            ticket.Subject = TicketSubjectPrefix + evt.Data.RenderedMessage;
            ticket.Description = evt.Data.RenderedMessage + "<br><br>" + string.Join("<br>", evt.Data.Properties.Select(p => $"{p.Key}: {p.Value}")) + "<br><br>" + evt.Data.Exception;

            switch(evt.Data.Level)
            {
                case LogEventLevel.Debug:
                case LogEventLevel.Verbose:
                case LogEventLevel.Information:
                    ticket.Priority = TicketPriority.Low;
                    break;

                case LogEventLevel.Warning:
                    ticket.Priority = TicketPriority.Medium;
                    break;

                case LogEventLevel.Error:
                    ticket.Priority = TicketPriority.High;
                    break;

                case LogEventLevel.Fatal:
                    ticket.Priority = TicketPriority.Urgent;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(TicketType))
            {
                ticket.Type = TicketType;
            }

            try
            {
                var response = _client.PostTicket(ticket);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    if (response.ErrorException != null)
                    {
                        throw response.ErrorException;
                    }

                    Log.ForContext("Content", response.Content).ForContext("Ticket", ticket, true).Warning("Freshdesk did not respond with the expected status {ExpectedStatus} but instead responded with {ActualStatus}", HttpStatusCode.Created, response.StatusCode);
                }
            } catch (Exception e)
            {
                Log.Error(e, "Failed to create ticket with freshdesk");
            }
        }

        protected override void OnAttached()
        {
            _client = new FreshdeskClient(Subdomain, Identifier, Password);

            base.OnAttached();
        }
        
        /// <summary>
        /// Used for running the reactor manually.
        /// </summary>
        public void Attach()
        {
            OnAttached();
        }
    }
}

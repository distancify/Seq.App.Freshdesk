using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Seq.App.Freshdesk
{
    [SeqApp("Freshdesk", Description = "Creates tickets in freshdesk for events.")]
    public class FreshdeskReactor : SeqApp, ISubscribeTo<LogEventData>
    {
        private FreshdeskClient _client;
        private ConcurrentDictionary<string, long> _ticketGroupIds = new ConcurrentDictionary<string, long>();

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

        [SeqAppSetting(DisplayName = "Group By Property", HelpText = "If this property is set on an event, new events will be posted as replies to previously created ticket with same value", IsOptional = true)]
        public string GroupByProperty { get; set; } = "";

        public void On(Event<LogEventData> evt)
        {
            bool isGrouped = !string.IsNullOrWhiteSpace(this.GroupByProperty) &&
                evt.Data.Properties.ContainsKey(this.GroupByProperty);
            string groupKey = isGrouped ? evt.Data.Properties[this.GroupByProperty].ToString() : null;
            if (string.IsNullOrWhiteSpace(groupKey))
            {
                groupKey = null;
                isGrouped = false;
            }

            if (isGrouped && _ticketGroupIds.ContainsKey(groupKey))
            {
                this.CreateReply(evt, groupKey);
            }
            else
            {
                this.CreateTicket(evt, isGrouped, groupKey);
            }
        }

        private void CreateTicket(Event<LogEventData> evt, bool isGrouped, string groupKey)
        {
            var ticket = new Ticket();
            ticket.Email = this.RequesterEmail;
            ticket.Name = this.RequesterName;
            ticket.Subject = this.TicketSubjectPrefix + evt.Data.RenderedMessage;
            ticket.Description = this.ToBody(evt);

            switch (evt.Data.Level)
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

            if (!string.IsNullOrWhiteSpace(this.TicketType))
            {
                ticket.Type = this.TicketType;
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

                    this.Log.ForContext("Content", response.Content).ForContext("Ticket", ticket, true).Warning("Freshdesk did not respond with the expected status {ExpectedStatus} but instead responded with {ActualStatus}", HttpStatusCode.Created, response.StatusCode);
                }
                else if (isGrouped)
                {
                    _ticketGroupIds.TryAdd(groupKey, response.Data.Id);
                }
            }
            catch (Exception e)
            {
                this.Log.Error(e, "Failed to create ticket in Freshdesk");
            }
        }

        private void CreateReply(Event<LogEventData> evt, string groupKey)
        {
            var reply = new Reply
            {
                Body = this.ToBody(evt)
            };
            try
            {
                var response = _client.PostReply(_ticketGroupIds[groupKey], reply);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    if (response.ErrorException != null)
                    {
                        throw response.ErrorException;
                    }

                    this.Log.ForContext("Content", response.Content).ForContext("Reply", reply, true).Warning("Freshdesk did not respond with the expected status {ExpectedStatus} but instead responded with {ActualStatus}", HttpStatusCode.Created, response.StatusCode);
                }
            }
            catch (Exception e)
            {
                this.Log.Error(e, "Failed to create reply in Freshdesk");
            }
        }

        private string ToBody(Event<LogEventData> evt)
        {
            return evt.Data.RenderedMessage + "<br><br>" + string.Join("<br>", evt.Data.Properties.Select(p => $"{p.Key}: {p.Value}")) + "<br><br>" + evt.Data.Exception;
        }

        protected override void OnAttached()
        {
            _client = new FreshdeskClient(this.Subdomain, this.Identifier, this.Password);

            base.OnAttached();
        }

        /// <summary>
        /// Used for running the reactor manually.
        /// </summary>
        public void Attach()
        {
            this.OnAttached();
        }
    }
}
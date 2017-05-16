using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;

namespace Seq.App.Freshdesk
{
    public class FreshdeskClient
    {
        private RestClient _client;

        public FreshdeskClient(string subdomain, string identifier, string password = null)
        {
            // Freshdesk requires moar stronger security than .net 4.5s default settings.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _client = new RestClient($"https://{subdomain}.freshdesk.com");
            _client.Authenticator = new HttpBasicAuthenticator(identifier, string.IsNullOrWhiteSpace(password) ? "X" : password);
        }

        public IRestResponse PostTicket(Ticket ticket)
        {
            var req = new RestRequest("api/v2/tickets", Method.POST);

            var body = new {
                name = ticket.Name,
                email = ticket.Email,
                priority = ticket.Priority,
                status = (int)TicketStatus.Open,
                source = (int)TicketSource.Email,
                subject = ticket.Subject.Substring(0, Math.Min(255, ticket.Subject.Length)),
                type = ticket.Type,
                description = ticket.Description
            };

            req.AddJsonBody(body);

            return _client.Execute(req);
        }
    }

    public class Ticket
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public TicketPriority Priority { get; set; }
        public string Type { get; set; }
    }
    
    public enum TicketSource
    {
        Email = 1
    }

    public enum TicketStatus
    {
        Open = 2
    }

    public enum TicketPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Urgent = 4
    }
    
}

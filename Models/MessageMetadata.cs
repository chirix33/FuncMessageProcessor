using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Models
{
    public class MessageMetadata
    {
        public EnrichedMessage? Reply { get; set; }
        public User? SendAs { get; set; }
        public User? ReplyBy { get; set; }
        public User? RepliedAs { get; set; }
        public User? AssignedBy { get; set; }
        public User? AssignedTo { get; set; }
        public User? RespondAs { get; set; }
        public GmailLabels? GmailLabels { get; set; }
        public Thread? Thread { get; set; }
        public RepliesTo? RepliesTo { get; set; }
        public string? Source { get; set; }
        public string? Type { get; set; }
        public string? TextContent { get; set; }
        public string? MessageId { get; set; }
        public bool? OriginalMessage { get; set; }
        public DateTime? RepliedOn { get; set; }
        public DateTime? ImportOn { get; set; }
        public bool? Deleted { get; set; }
    }

    public class User
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Signature { get; set; }
        public string? Email { get; set; }
        public bool? Active { get; set; }
    }

    public class RepliesTo
    {
        public string? Id { get; set; }
        public string? MessageId { get; set; }
    }

    public class Thread
    {
        public string? Id { get; set; }
        public string? Topic { get; set; }
        public string? LatestText { get; set; }
    }

    public class GmailLabels
    {
        public List<string>? User { get; set; }
        public List<string>? System { get; set; }
    }
}

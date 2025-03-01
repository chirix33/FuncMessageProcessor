using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Services
{
    public interface ICosmosUtilsService
    {
        /// <summary>
        /// Fetches the ID of the parent message from Cosmos DB.
        /// </summary>
        /// <param name="messageFromAddress">The email address of the sender.</param>
        /// <param name="messageSubject">The subject of the message.</param>
        /// <returns>The ID of the parent message, or null if not found.</returns>
        string? GetParentMessages(string messageFromAddress, string messageSubject);

        /// <summary>
        /// Fetches the parent ID from Cosmos DB for given message details.
        /// </summary>
        /// <param name="messageFromAddress">The email address of the sender.</param>
        /// <param name="messageSubject">The subject of the message.</param>
        /// <returns>The parent ID, or null if not found.</returns>
        string? GetParentId(string messageFromAddress, string messageSubject);

        /// <summary>
        /// Fetches the body content of a message by its ID from Cosmos DB.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        /// <returns>The body content of the message, or null if not found.</returns>
        string? GetMessageBodyById(string messageId);

        /// <summary>
        /// Fethes the body content of the last 3 parent messages from Cosmos DB.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        /// <returns>The body content of the last 3 parent messages, or null if not found.</returns>
        public string GetLastThreeParentMessages(string messageId);
    }
}

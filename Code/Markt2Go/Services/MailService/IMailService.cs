using System.Collections.Generic;
using System.Threading.Tasks;

namespace Markt2Go.Services.MailService
{
    public interface IMailService
    {
        Task SendReservationConfirmation(string recipientMail, Dictionary<string, string> placeholders);
        Task SendReservationAccepted(string recipientMail, Dictionary<string, string> placeholders);
        Task SendReservationDeclined(string recipientMail, Dictionary<string, string> placeholders);
        Task SendReservationPacked(string recipientMail, Dictionary<string, string> placeholders);
    }
}
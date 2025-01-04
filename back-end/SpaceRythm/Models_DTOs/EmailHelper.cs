using System.Net.Mail;
using System.Net;
using System.Security.Policy;
namespace SpaceRythm.Models;


public class EmailHelper
{
    public EmailHelperOptions Options { get; }
    public EmailHelper(EmailHelperOptions options)
    {
        Options = options;
    }

    public bool SendEmail(string userEmail, string confirmationLink, string subject)
    {
        MailMessage mailMessage = new()
        {
            From = new MailAddress(Options.From)
        };
        mailMessage.To.Add(new MailAddress(userEmail));
        mailMessage.Subject = subject;
        mailMessage.IsBodyHtml = true;
        mailMessage.Body = confirmationLink;

        SmtpClient client = new()
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(Options.Login, Options.Password),
            Host = Options.Host,
            Port = Options.Port,
            EnableSsl = Options.EnableSSL
        };

        try
        {
            client.Send(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }
   
    public bool SendEmailResetPassword(string userEmail, string resetLink)
    {
        return SendEmail(userEmail, resetLink, "Скидання паролю");
    }
}


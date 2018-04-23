using System.IO;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.Collections.Generic;
using System.ServiceModel.Activation;

[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
public class MailService : IMailService {
    public void SendMail(string mailHost, string from, string to, string subject, string body, List<AttachementInfo> attachments) {
        SmtpClient client = new SmtpClient(mailHost, 25);
        MailAddress afrom = new MailAddress(from, "RichEditClient", System.Text.Encoding.UTF8);
        MailAddress ato = new MailAddress(to);
        MailMessage message = new MailMessage(from, to);

        message.Subject = subject;

        AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, MediaTypeNames.Text.Html);

        for (int i = 0; i < attachments.Count; i++) {
            AttachementInfo info = attachments[i];
            LinkedResource resource = new LinkedResource(new MemoryStream(info.Data), info.MimeType);
            resource.ContentId = info.ContentId;
            htmlView.LinkedResources.Add(resource);
        }

        message.AlternateViews.Add(htmlView);
        message.IsBodyHtml = true;

        // Specify your login/password to log on to the SMTP server, if required
        //client.Credentials = new System.Net.NetworkCredential("login", "password");
        
        client.Send(message);
    }
}
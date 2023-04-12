using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Utilidades
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(subject, htmlMessage, email);
        }

        public Task Execute(string subject, string mensaje, string email)
        {
            string correo = _configuration.GetValue<string>("EmailSender:Correo");
            string apikey = _configuration.GetValue<string>("EmailSender:ApiKey");

            MailMessage mm = new MailMessage();
            mm.To.Add(email);
            mm.Subject = subject;
            mm.Body = mensaje;
            mm.From = new MailAddress(correo);
            mm.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp.sendgrid.net");
            smtp.Port = 587;
            smtp.UseDefaultCredentials= true;
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("apikey", apikey);

            return smtp.SendMailAsync(mm);
        }
    }
}

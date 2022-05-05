using Newtonsoft.Json;
using smtpServer;
using System;

namespace smtpServer
{
    public static class EmailSaver
    {
        private static string EmailDir = "emails/";

        public static void Save(Email email)
        {
            var recipients = email.GetRecipientList();
            foreach (var recipient in recipients)
            {
                List<Email> emailList = new List<Email>();
                if (DoesUserDirExist(recipient.Name))
                {
                    emailList.AddRange(GetUserEmails(recipient.Name));
                }
                if (recipient.Type == RecievingType.Bcc)
                {
                    var bccMail = new Email(email);
                    emailList.Add(bccMail);
                }
                else
                {
                    emailList.Add(email);
                }
                WriteEmails(Newtonsoft.Json.JsonConvert.SerializeObject(emailList), $"{EmailDir}{recipient.Name}.json");
            }
        }

        private static void WriteEmails(string json, string path)
        {
            using StreamWriter file = new(path);
            file.Write(json);
        }

        private static bool DoesUserDirExist(string name)
        {
            return File.Exists($"{EmailDir.ToString()}{name}.json");
        }

        public static List<Email> GetUserEmails(string name)
        {
            using (StreamReader r = new StreamReader($"{EmailDir.ToString()}{name}.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<List<Email>>(json);
            }
        }

    }
}
from smtplib import SMTP
 
if __name__ == "__main__":
    with SMTP("127.0.0.1", 8000) as smtp:
        # smtp.debuglevel = 1
 
        print("helo")
        print(smtp.helo())
 
        print("ehlo")
        print(smtp.ehlo())
 
        print("noop")
        print(smtp.noop())
 
        sender = "sender@test.com"
        print(f"vrfy {sender}")
        print(smtp.verify(sender))
 
        receivers = ["tomas@test.com", "jonas@test.com"]
        content = f"Cc:test@test.com,test@test.com\r\nBcc:testas@test.com\r\nThis is a test e-mail message."
        print("Sending mail")
        print(f"Errors: {smtp.sendmail(sender, receivers, content)}")
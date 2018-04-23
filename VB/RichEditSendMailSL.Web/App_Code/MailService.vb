Imports Microsoft.VisualBasic
Imports System.IO
Imports System.Text
Imports System.Net.Mail
Imports System.Net.Mime
Imports System.Collections.Generic
Imports System.ServiceModel.Activation

<AspNetCompatibilityRequirements(RequirementsMode := AspNetCompatibilityRequirementsMode.Required)> _
Public Class MailService
	Implements IMailService
	Public Sub SendMail(ByVal mailHost As String, ByVal [from] As String, ByVal [to] As String, ByVal subject As String, ByVal body As String, ByVal attachments As List(Of AttachementInfo)) Implements IMailService.SendMail
		Dim client As New SmtpClient(mailHost, 25)
		Dim afrom As New MailAddress(From, "RichEditClient", System.Text.Encoding.UTF8)
		Dim ato As New MailAddress([to])
		Dim message As New MailMessage(From, [to])

		message.Subject = subject

		Dim htmlView As AlternateView = AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, MediaTypeNames.Text.Html)

		For i As Integer = 0 To attachments.Count - 1
			Dim info As AttachementInfo = attachments(i)
			Dim resource As New LinkedResource(New MemoryStream(info.Data), info.MimeType)
			resource.ContentId = info.ContentId
			htmlView.LinkedResources.Add(resource)
		Next i

		message.AlternateViews.Add(htmlView)
		message.IsBodyHtml = True

		' Specify your login/password to log on to the SMTP server, if required
		'client.Credentials = new System.Net.NetworkCredential("login", "password");

		client.Send(message)
	End Sub
End Class
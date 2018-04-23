Imports Microsoft.VisualBasic
Imports System.Text
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Controls
Imports DevExpress.Utils
Imports DevExpress.Xpf.RichEdit
Imports DevExpress.XtraRichEdit
Imports DevExpress.XtraRichEdit.Export
Imports DevExpress.XtraRichEdit.Services
Imports DevExpress.XtraRichEdit.Utils
Imports System.Collections.Generic
Imports RichEditSendMailSL.MailServiceReference
Imports System.ServiceModel
Imports System.ComponentModel
Imports DevExpress.Office.Utils

Namespace RichEditSendMailSL
    Partial Public Class MainPage
        Inherits UserControl
        Public Sub New()
            InitializeComponent()

            InitializeDefaultMessage()
        End Sub

        Private Sub InitializeDefaultMessage()
            tbTo.Text = "someone@somewhere.com"
            tbSubject.Text = "Test message"
            tbMailServer.Text = "mail.somewhere.com"
            reMessageBody.LoadDocument(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Hello.docx"), DocumentFormat.OpenXml)
        End Sub

        Private Sub btnSend_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
            Dim exporter As New RichEditMailMessageExporter(reMessageBody)
            exporter.Export()

            Dim client As New MailServiceClient()

            AddHandler client.SendMailCompleted, Function(s, ea) AnonymousMethod1(s, ea)

            client.SendMailAsync(tbMailServer.Text, "DXRichEdit@devexpress.com", tbTo.Text, tbSubject.Text, exporter.Html, exporter.Attachments)
        End Sub

        Private Function AnonymousMethod1(ByVal s As Object, ByVal ea As AsyncCompletedEventArgs) As Boolean
            If TypeOf ea.Error Is FaultException(Of ExceptionDetail) Then
                Dim fault As FaultException(Of ExceptionDetail) = TryCast(ea.Error, FaultException(Of ExceptionDetail))
                MessageBox.Show(String.Format("{0}:" & Constants.vbCrLf & "{1}", fault.Detail.Type, fault.Detail.Message))
            ElseIf ea.Error IsNot Nothing Then
                MessageBox.Show(ea.Error.Message)
            Else
                MessageBox.Show("Message was sent successfully!")
            End If
            Return True
        End Function
    End Class

    Public Class RichEditMailMessageExporter
        Implements IUriProvider
        Private ReadOnly control As RichEditControl
        Private imageId As Integer

        Private privateHtml As String
        Public Property Html() As String
            Get
                Return privateHtml
            End Get
            Private Set(ByVal value As String)
                privateHtml = value
            End Set
        End Property
        Private privateAttachments As List(Of AttachementInfo)
        Public Property Attachments() As List(Of AttachementInfo)
            Get
                Return privateAttachments
            End Get
            Private Set(ByVal value As List(Of AttachementInfo))
                privateAttachments = value
            End Set
        End Property

        Public Sub New(ByVal control As RichEditControl)
            Guard.ArgumentNotNull(control, "control")

            Me.control = control
        End Sub

        Public Overridable Sub Export()
            Me.Attachments = New List(Of AttachementInfo)()
            AddHandler control.BeforeExport, AddressOf OnBeforeExport
            Me.Html = control.Document.GetHtmlText(control.Document.Range, Me)
            RemoveHandler control.BeforeExport, AddressOf OnBeforeExport
        End Sub

        Private Sub OnBeforeExport(ByVal sender As Object, ByVal e As BeforeExportEventArgs)
            Dim options As HtmlDocumentExporterOptions = TryCast(e.Options, HtmlDocumentExporterOptions)
            If options IsNot Nothing Then
                options.Encoding = Encoding.UTF8
            End If
        End Sub

#Region "IUriProvider Members"

        Public Function CreateCssUri(ByVal rootUri As String, ByVal styleText As String, ByVal relativeUri As String) As String Implements IUriProvider.CreateCssUri
            Return String.Empty
        End Function

        Public Function CreateImageUri(ByVal rootUri As String, ByVal image As OfficeImage, ByVal relativeUri As String) As String Implements IUriProvider.CreateImageUri
            Dim imageName As String = String.Format("image{0}", imageId)
            imageId += 1

            Dim imageFormat As OfficeImageFormat = GetActualImageFormat(image.RawFormat)
            Dim data1() As Byte = image.GetImageBytes(imageFormat)
            Dim mediaContentType As String = OfficeImage.GetContentType(imageFormat)
            Dim info As New AttachementInfo() With {.Data = data1, .MimeType = mediaContentType, .ContentId = imageName}
            Me.Attachments.Add(info)

            Return "cid:" & imageName
        End Function

        Private Function GetActualImageFormat(ByVal imageFormat As OfficeImageFormat) As OfficeImageFormat
            If imageFormat = OfficeImageFormat.Exif OrElse imageFormat = OfficeImageFormat.MemoryBmp Then
                Return OfficeImageFormat.Png
            Else
                Return imageFormat
            End If
        End Function
#End Region
    End Class

End Namespace
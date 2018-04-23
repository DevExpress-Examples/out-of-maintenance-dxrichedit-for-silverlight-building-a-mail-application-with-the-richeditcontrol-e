using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Utils;
using DevExpress.Xpf.RichEdit;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.Export;
using DevExpress.XtraRichEdit.Services;
using DevExpress.XtraRichEdit.Utils;
using System.Collections.Generic;
using RichEditSendMailSL.MailServiceReference;
using System.ServiceModel;

namespace RichEditSendMailSL {
    public partial class MainPage : UserControl {
        public MainPage() {
            InitializeComponent();

            InitializeDefaultMessage();
        }

        private void InitializeDefaultMessage() {
            tbTo.Text = "someone@somewhere.com";
            tbSubject.Text = "Test message";
            tbMailServer.Text = "mail.somewhere.com";
            reMessageBody.LoadDocument(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("RichEditSendMailSL.Hello.docx"),
                DocumentFormat.OpenXml);
        }

        private void btnSend_Click(object sender, System.Windows.RoutedEventArgs e) {
            RichEditMailMessageExporter exporter = new RichEditMailMessageExporter(reMessageBody);
            exporter.Export();

            MailServiceClient client = new MailServiceClient();

            client.SendMailCompleted += (s, ea) => {
                if (ea.Error is FaultException<ExceptionDetail>) {
                    FaultException<ExceptionDetail> fault = ea.Error as FaultException<ExceptionDetail>;
                    MessageBox.Show(string.Format("{0}:\r\n{1}", fault.Detail.Type, fault.Detail.Message));
                }
                else if (ea.Error != null) {
                    MessageBox.Show(ea.Error.Message);
                }
                else {
                    MessageBox.Show("Message was sent successfully!");
                }
            };

            client.SendMailAsync(tbMailServer.Text, "DXRichEdit@devexpress.com", tbTo.Text, tbSubject.Text, exporter.Html, exporter.Attachments);
        }
    }
    
    public class RichEditMailMessageExporter : IUriProvider {
        readonly RichEditControl control;
        int imageId;

        public string Html { get; private set; }
        public List<AttachementInfo> Attachments { get; private set; }

        public RichEditMailMessageExporter(RichEditControl control) {
            Guard.ArgumentNotNull(control, "control");

            this.control = control;
        }

        public virtual void Export() {
            this.Attachments = new List<AttachementInfo>();
            control.BeforeExport += OnBeforeExport;
            this.Html = control.Document.GetHtmlText(control.Document.Range, this);
            control.BeforeExport -= OnBeforeExport;
        }

        void OnBeforeExport(object sender, BeforeExportEventArgs e) {
            HtmlDocumentExporterOptions options = e.Options as HtmlDocumentExporterOptions;
            if (options != null) {
                options.Encoding = Encoding.UTF8;
            }
        }

        #region IUriProvider Members

        public string CreateCssUri(string rootUri, string styleText, string relativeUri) {
            return string.Empty;
        }

        public string CreateImageUri(string rootUri, RichEditImage image, string relativeUri) {
            string imageName = string.Format("image{0}", imageId);
            imageId++;

            RichEditImageFormat imageFormat = GetActualImageFormat(image.RawFormat);
            byte[] data = image.GetImageBytes(imageFormat);
            string mediaContentType = RichEditImage.GetContentType(imageFormat);
            AttachementInfo info = new AttachementInfo() {
                Data = data,
                MimeType = mediaContentType,
                ContentId = imageName
            };
            this.Attachments.Add(info);

            return "cid:" + imageName;
        }

        RichEditImageFormat GetActualImageFormat(RichEditImageFormat imageFormat) {
            if (imageFormat == RichEditImageFormat.Exif ||
                imageFormat == RichEditImageFormat.MemoryBmp)
                return RichEditImageFormat.Png;
            else
                return imageFormat;
        }
        #endregion
    }
     
}
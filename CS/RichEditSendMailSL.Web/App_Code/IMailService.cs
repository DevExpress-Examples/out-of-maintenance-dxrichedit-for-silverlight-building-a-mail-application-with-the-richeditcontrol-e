using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

[ServiceContract]
public interface IMailService {
    [OperationContract]
    void SendMail(string mailHost, string from, string to, string subject, string body, List<AttachementInfo> attachments);
}

[DataContract]
public class AttachementInfo {
    [DataMember]
    public byte[] Data { get; set; }
    [DataMember]
    public string MimeType { get; set; }
    [DataMember]
    public string ContentId { get; set; }
}
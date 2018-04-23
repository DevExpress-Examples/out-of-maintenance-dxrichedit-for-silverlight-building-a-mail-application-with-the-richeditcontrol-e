Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Runtime.Serialization
Imports System.ServiceModel

<ServiceContract> _
Public Interface IMailService
	<OperationContract> _
	Sub SendMail(ByVal mailHost As String, ByVal [from] As String, ByVal [to] As String, ByVal subject As String, ByVal body As String, ByVal attachments As List(Of AttachementInfo))
End Interface

<DataContract> _
Public Class AttachementInfo
	Private privateData As Byte()
	<DataMember> _
	Public Property Data() As Byte()
		Get
			Return privateData
		End Get
		Set(ByVal value As Byte())
			privateData = value
		End Set
	End Property
	Private privateMimeType As String
	<DataMember> _
	Public Property MimeType() As String
		Get
			Return privateMimeType
		End Get
		Set(ByVal value As String)
			privateMimeType = value
		End Set
	End Property
	Private privateContentId As String
	<DataMember> _
	Public Property ContentId() As String
		Get
			Return privateContentId
		End Get
		Set(ByVal value As String)
			privateContentId = value
		End Set
	End Property
End Class
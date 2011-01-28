Imports System.Xml.Serialization
Imports System.Runtime.Serialization

<Serializable()>
<XmlRoot()>
<DataContract()>
Public Class ShortenReturn

    <XmlElement("id")>
    <DataMember()>
    Public Property id As String

    <XmlElement("kind")> <DataMember()>
    Public Property kind As String

    <XmlElement("longUrl")> <DataMember()>
    Public Property longUrl As String

End Class

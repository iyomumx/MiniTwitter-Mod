Imports System.Xml.Serialization
Imports System.Runtime.Serialization

<Serializable()>
<XmlRoot()>
<DataContract()>
Public Class ExpandReutrn

    <XmlElement("id")>
    <DataMember()>
    Public Property id As String

    <XmlElement("kind")> <DataMember()>
    Public Property kind As String

    <XmlElement("longUrl")> <DataMember()>
    Public Property longUrl As String

    <XmlElement("status"), DataMember()>
    Public Property status As String

End Class

Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web
Imports System.Xml.Serialization
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization.Json

Public Enum HttpVerbs
    [GET]
    POST
    PUT
    DELETE
End Enum

Public Class OAuthBase
    Protected Sub New(ByVal consumerKey As String, ByVal consumerSecret As String)
        _consumerKey = consumerKey
        _consumerSecret = consumerSecret
    End Sub

    Protected _token As String
    Protected _tokenSecret As String

    Private ReadOnly _consumerKey As String
    Private ReadOnly _consumerSecret As String

    Public Shared Endpoint As String = "https://www.google.com/accounts/"
    Public Shared ReadOnly SignEndpoint As String = "https://www.google.com/accounts/"
    Private Shared ReadOnly _unixEpoch As New DateTime(1970, 1, 1, 0, 0, 0, 0)

    Private Const _unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~"

    Private Class Serializer(Of T)
        Private Shared ReadOnly _xs As New DataContractJsonSerializer(GetType(T))

        Public Shared Function Deserizlize(ByVal stream As Stream) As T
            Return CType(_xs.ReadObject(stream), T)
        End Function

        Public Shared Sub Serizlize(ByVal obj As T, ByVal stream As Stream)
            _xs.WriteObject(stream, obj)
        End Sub
    End Class

#Region "实例方法"

    Protected Function [Get](ByVal url As String) As String
        Return [Get](url, Nothing)
    End Function

    Protected Function [Get](ByVal url As String, ByVal param As Object) As String
        Return Fetch(HttpVerbs.GET, url, param, _token, _tokenSecret, Nothing)
    End Function

    Protected Function [Get](Of T As Class)(ByVal url As String) As T
        Return [Get](Of T)(url, Nothing)
    End Function

    Protected Function [Get](Of T As Class)(ByVal url As String, ByVal param As Object) As T
        Return Fetch(Of T)(HttpVerbs.GET, url, param, _token, _tokenSecret, Nothing)
    End Function

    Protected Function Post(ByVal url As String) As String
        Return Post(url, Nothing)
    End Function

    Protected Function Post(ByVal url As String, ByVal param As Object) As String
        Return Fetch(HttpVerbs.POST, url, param, _token, _tokenSecret, Nothing)
    End Function

    Protected Function Post(Of T As Class)(ByVal url As String) As T
        Return Post(Of T)(url, Nothing)
    End Function

    Protected Function Put(ByVal url As String) As String
        Return Post(url, Nothing)
    End Function

    Protected Function Put(ByVal url As String, ByVal param As Object) As String
        Return Fetch(HttpVerbs.PUT, url, param, _token, _tokenSecret, Nothing)
    End Function

    Protected Function Post(Of T As Class)(ByVal url As String, ByVal param As Object) As T
        Return Fetch(Of T)(HttpVerbs.POST, url, param, _token, _tokenSecret, Nothing)
    End Function

    Protected Function Put(Of T As Class)(ByVal url As String) As T
        Return Post(Of T)(url, Nothing)
    End Function

    Protected Function Put(Of T As Class)(ByVal url As String, ByVal param As String) As T
        Return Fetch(Of T)(HttpVerbs.PUT, url, param, _token, _tokenSecret, Nothing)
    End Function

    Protected Function Delete(ByVal url As String) As String
        Return Post(url, Nothing)
    End Function

    Protected Function Delete(ByVal url As String, ByVal param As Object) As String
        Return Fetch(HttpVerbs.DELETE, url, param, _token, _tokenSecret, Nothing)
    End Function

    Protected Function Delete(Of T As Class)(ByVal url As String) As T
        Return Post(Of T)(url, Nothing)
    End Function

    Protected Function Delete(Of T As Class)(ByVal url As String, ByVal param As String) As T
        Return Fetch(Of T)(HttpVerbs.DELETE, url, param, _token, _tokenSecret, Nothing)
    End Function
#End Region

#Region "内部方法"

    Protected Function Fetch(ByVal verb As HttpVerbs, ByVal url As String, ByVal param As Object, ByVal token As String, ByVal tokenSecret As String, ByVal Verifier As String) As String
        Dim temp As DateTime
        Return Fetch(verb, url, param, token, tokenSecret, Verifier, temp)
    End Function

    Protected Function Fetch(ByVal verb As HttpVerbs, ByVal url As String, ByVal param As Object, ByVal token As String, ByVal tokenSecret As String, ByVal Verifier As String, ByRef lastModified As DateTime) As String
        lastModified = Now

        For i As Integer = 0 To 4
            Dim Request As HttpWebRequest = CreateRequest(verb, url, param, token, tokenSecret, Verifier)
            Try
                Dim response As HttpWebResponse = Request.GetResponse()
                lastModified = response.LastModified
                Using reader = New StreamReader(response.GetResponseStream)
                    Return reader.ReadToEnd
                End Using
            Catch ex As WebException
                If ex.Status = WebExceptionStatus.ProtocolError Then
                    Dim response As HttpWebResponse = ex.Response
                    If response.StatusCode < 500 Then
                        Using reader = New StreamReader(response.GetResponseStream)
                            Dim s = reader.ReadToEnd
                            Throw New ApplicationException(s, ex)
                        End Using
                    End If
                End If
            Catch
                Return Nothing
            End Try
        Next
        Return Nothing
    End Function

    Protected Function Fetch(Of T)(ByVal verb As HttpVerbs, ByVal url As String, ByVal param As Object, ByVal token As String, ByVal tokenSecret As String, ByVal Verifier As String) As T
        Dim temp As DateTime
        Return Fetch(Of T)(verb, url, param, token, tokenSecret, Verifier, temp)
    End Function

    Protected Function Fetch(Of T)(ByVal verb As HttpVerbs, ByVal url As String, ByVal param As Object, ByVal token As String, ByVal tokenSecret As String, ByVal Verifier As String, ByRef lastModified As DateTime) As T
        lastModified = Now

        For i As Integer = 0 To 4
            Dim Request As HttpWebRequest = CreateRequestJ(verb, url, param, token, tokenSecret, Verifier)
            Try
                Dim response As HttpWebResponse = Request.GetResponse()
                lastModified = response.LastModified
                Using stream = response.GetResponseStream
                    Return Serializer(Of T).Deserizlize(stream)
                End Using
            Catch ex As WebException
                If ex.Status = WebExceptionStatus.ProtocolError Then
                    Dim response As HttpWebResponse = ex.Response
                    If response.StatusCode < 500 Then
                        Using streamReader = New StreamReader(response.GetResponseStream)
                            Dim errInfo As String = streamReader.ReadToEnd
                            Throw New ApplicationException(errInfo, ex)
                        End Using
                    End If
                End If
            Catch
                Return Nothing
            End Try
        Next
        Return Nothing
    End Function

#End Region

#Region "StreamAPI 相关"
    Public Function FetchStream(ByVal verb As HttpVerbs, ByVal url As String, ByVal param As Object, ByRef lastModified As DateTime) As Stream
        lastModified = Now

        For i As Integer = 0 To 4
            Dim Request As HttpWebRequest = CreateRequest(verb, url, param, _token, _tokenSecret, Nothing)
            Try
                Dim response As HttpWebResponse = Request.GetResponse()
                lastModified = response.LastModified
                Return response.GetResponseStream
            Catch ex As WebException
                If ex.Status = WebExceptionStatus.ProtocolError Then
                    Dim response As HttpWebResponse = ex.Response
                    If response.StatusCode < 500 Then
                        Throw
                    End If
                End If
            Catch
                Return Nothing
            End Try
        Next
        Return Nothing
    End Function
#End Region

#Region "OAuth"
    Public Function RedirectToAuthorize(ByVal token As String) As String
        Return String.Format("{0}?oauth_token={1}", Endpoint + "OAuthAuthorizeToken", token)
    End Function

    Public Function GetRequestToken(ByRef token As String, Optional ByVal param As Object = Nothing, Optional ByRef tokenSecret As String = "") As Boolean
        Dim response = Fetch(HttpVerbs.POST, Endpoint + "OAuthGetRequestToken", param, Nothing, Nothing, Nothing)
        If response.IsNullOrEmpty Then
            token = Nothing
            Return False
        End If
        Dim query = ParseQueryString(response)
        token = UrlDecode(query("oauth_token"))
        tokenSecret = UrlDecode(query("oauth_token_secret"))
        Return True
    End Function

    Public Function GetAccessToken(ByRef token As String, ByRef tokenSecret As String, ByVal verifier As String, Optional ByVal param As Object = Nothing) As Boolean
        Dim response = Fetch(HttpVerbs.POST, Endpoint + "OAuthGetAccessToken", param, token, tokenSecret, verifier)
        If response.IsNullOrEmpty Then
            Return False
        End If
        Dim query = ParseQueryString(response)
        token = UrlDecode(query("oauth_token"))
        tokenSecret = UrlDecode(query("oauth_token_secret"))
        Return True
    End Function

    Public Function GetAccessToken(ByVal username As String, ByVal password As String, ByRef token As String, ByRef tokenSecret As String) As Boolean
        Try
            Dim response = Fetch(HttpVerbs.POST, SignEndpoint + "oauth/access_token", New With {.x_auth_mode = "client_auth", .x_auth_password = password, .x_auth_username = username}, Nothing, Nothing, Nothing)
            If response.IsNullOrEmpty Then
                Return False
            End If
            Dim query = ParseQueryString(response)
            token = query("oauth_token")
            tokenSecret = query("oauth_token_secret")
            If token.IsNullOrEmpty Or tokenSecret.IsNullOrEmpty Then
                Return False
            End If
        Catch
            Return False
        End Try

        Return True
    End Function
#End Region

#Region "辅助方法"
    Private Function CreateRequest(ByVal verb As HttpVerbs, ByVal url As String, ByVal param As Object, ByVal token As String, ByVal tokenSecret As String, ByVal Verifier As String) As HttpWebRequest
        Dim query = ParseQueryObject(param)
        Dim removeP = False
        If query.AllKeys.Contains("oauth_callback") Then
            query.Remove("oauth_callback")
            removeP = True
        End If
        Dim queryString = CreateQueryString(query)
        If removeP Then
            query.Add("oauth_callback", "oob")
        End If
        If verb = HttpVerbs.GET AndAlso (Not queryString.IsNullOrEmpty) Then
            url += "?" + queryString
        End If
        Dim request As HttpWebRequest = WebRequest.Create(url)
        request.Method = ToMethodString(verb)
        request.Accept = "application/json, text/json, */*"
        request.Timeout = 60000
        request.ServicePoint.Expect100Continue = False
        request.AutomaticDecompression =
            DecompressionMethods.Deflate Or DecompressionMethods.GZip

        If verb <> HttpVerbs.GET Then
            'request.ContentType = "application/x-www-form-urlencoded"
            request.ContentType = "application/x-www-form-urlencoded"
            Using writer = New StreamWriter(request.GetRequestStream)
                writer.Write(queryString)
            End Using
        End If
        AddOAuthToken(request, query, token, tokenSecret, Verifier)
        Return request
    End Function

    Private Function CreateRequestJ(ByVal verb As HttpVerbs, ByVal url As String, ByVal param As Object, ByVal token As String, ByVal tokenSecret As String, ByVal Verifier As String) As HttpWebRequest
        Dim query = ParseQueryObject(param)
        Dim queryString = CreateQueryStringJ(query)
        If verb = HttpVerbs.GET AndAlso (Not queryString.IsNullOrEmpty) Then
            url += "?" + queryString
        End If
        Dim request As HttpWebRequest = WebRequest.Create(url)
        request.Method = ToMethodString(verb)
        request.Accept = "*/*"
        request.Timeout = 60000
        request.ServicePoint.Expect100Continue = False
        request.Headers.Add("GData-Version", "2.0")
        If verb <> HttpVerbs.GET Then
            'request.ContentType = "application/x-www-form-urlencoded"
            request.ContentType = "application/json"
            'Dim nvc = ParseQueryString(queryString)
            'queryString = CreateQueryStringJ(nvc)
            Using writer = New StreamWriter(request.GetRequestStream)
                writer.Write(queryString)
            End Using
            query = Nothing
        End If
        AddOAuthToken(request, query, token, tokenSecret, Verifier)
        Return request
    End Function

    Private Sub AddOAuthToken(ByRef request As HttpWebRequest, ByVal query As NameValueCollection, ByVal token As String, ByVal tokenSecret As String, ByVal verifier As String)
        Dim parameter As New NameValueCollection
        parameter.Add("oauth_consumer_key", _consumerKey)
        parameter.Add("oauth_signature_method", "HMAC-SHA1")
        parameter.Add("oauth_timestamp", GetTimeStamp())
        parameter.Add("oauth_nonce", GetNonce())
        parameter.Add("oauth_version", "1.0")
        If Not verifier.IsNullOrEmpty Then
            parameter.Add("oauth_verifier", verifier)
        End If
        If Not token.IsNullOrEmpty Then
            parameter.Add("oauth_token", token)
        End If
        If query IsNot Nothing Then
            parameter.Add(query)
        End If
        Dim signature = CreateSignature(tokenSecret, request.Method.ToEnum, request.RequestUri, parameter)
        parameter.Add("oauth_signature", signature)
        Dim header As New StringBuilder
        header.Append("OAuth")
        For Each Key In (From p In parameter.AllKeys Where p.StartsWith("oauth_") Order By StringOrderer(p) Ascending)
            header.AppendFormat(" {0}=""{1}"",", Key, UrlEncode(parameter(Key)))
        Next Key
        request.Headers(HttpRequestHeader.Authorization) = header.ToString(0, header.Length - 1)
    End Sub

    Private Shared Function StringOrderer(ByVal str As String) As Integer
        Select Case str
            Case "oauth_version"
                Return 1
            Case "oauth_nonce"
                Return 2
            Case "oauth_consumer_key"
                Return 4
            Case "oauth_timestamp"
                Return 3
            Case "oauth_token"
                Return 5
            Case "oauth_signature_method"
                Return 6
            Case "oauth_signature"
                Return 7
            Case Else
                Return 99
        End Select
    End Function

    Private Shared Function ToMethodString(ByVal HttpVerbs As HttpVerbs) As String

        Select Case HttpVerbs
            Case HttpVerbs.POST
                Return "POST"
            Case HttpVerbs.DELETE
                Return "PUT"
            Case HttpVerbs.PUT
                Return "DELETE"
        End Select
        Return "GET"
    End Function

    Private Shared Function ParseQueryObject(ByVal component As Object) As NameValueCollection
        Dim NVC As New NameValueCollection()
        If component IsNot Nothing Then
            For Each prop As PropertyDescriptor In TypeDescriptor.GetProperties(component)
                NVC.Add(prop.Name, prop.GetValue(component).ToString)
            Next
        End If
        Return NVC
    End Function

    Private Shared Function CreateQueryString(ByVal Collection As NameValueCollection) As String
        If Collection.Count = 0 Then
            Return String.Empty
        End If
        Dim QueryString As New StringBuilder()
        For Each key In (From p In Collection.AllKeys Order By p)
            QueryString.AppendFormat("{0}={1}&", UrlEncode(key), UrlEncode(Collection(key)))
        Next
        Return QueryString.ToString(0, QueryString.Length - 1)
    End Function

    Private Shared Function CreateQueryStringJ(ByVal Collection As NameValueCollection) As String
        If Collection.Count = 0 Then
            Return String.Empty
        End If
        Dim QueryString As New StringBuilder()
        QueryString.Append("{")
        For Each key In Collection.AllKeys.OrderBy(Function(p) p)
            QueryString.AppendFormat("""{0}"":""{1}"",", key, Collection(key))
        Next
        Return QueryString.ToString(0, QueryString.Length - 1) + "}"
    End Function

    Private Shared ReadOnly decodeRegex As New Text.RegularExpressions.Regex("%[0-9A-F]{2}", RegularExpressions.RegexOptions.Compiled)
    Private Shared Function UrlDecode(ByVal url As String) As String
        Dim sb As New StringBuilder(url)
        For Each match As RegularExpressions.Match In decodeRegex.Matches(url)
            sb.Replace(match.Value, Encoding.UTF8.GetChars({CByte("&H" + match.Value(1) + match.Value(2))}))
        Next
        Return sb.ToString
    End Function

    Private Shadows Function ParseQueryString(ByVal queryString As String) As NameValueCollection
        Dim query As New NameValueCollection
        Dim parts = queryString.Split("&"c)
        For Each part In parts
            Dim index = part.IndexOf("="c)
            If index = -1 Then
                query.Add(part, String.Empty)
            Else
                query.Add(part.Substring(0, index), part.Substring(index + 1))
            End If
        Next
        Return query
    End Function

    Private Shared Function UrlEncode(ByVal str As String) As String
        If str.IsNullOrEmpty Then Return String.Empty
        Dim sb As New StringBuilder()
        Dim Bytes = Encoding.UTF8.GetBytes(str)
        For Each b In Bytes
            If _unreservedChars.Contains(Encoding.UTF8.GetChars({b})(0)) Then
                sb.Append(Encoding.UTF8.GetChars({b})(0))
            Else
                sb.AppendFormat("%{0:X2}", b)
            End If
        Next
        Return sb.ToString
    End Function

    Protected Friend Function GetNonce() As String
        Return Guid.NewGuid.ToString("N")
    End Function

    Protected Friend Function GetTimeStamp() As String
        Dim ts As TimeSpan = DateTime.UtcNow - _unixEpoch
        Return Convert.ToInt64(ts.TotalSeconds).ToString()
    End Function

    Public Function CreateSignature(ByVal tokenSecret As String,
                         ByVal method As HttpVerbs, ByVal uri As Uri,
                         ByVal query As NameValueCollection) As String
        Dim url = String.Format("{0}://{1}{2}", uri.Scheme, uri.Host, uri.AbsolutePath)
        Dim addtionalQuery As String = uri.Query.Substring(1)
        If Not String.IsNullOrEmpty(addtionalQuery) Then
            If query Is Nothing Then
                query = New NameValueCollection()
            End If
            query.Add(ParseQueryString(addtionalQuery))
        End If
        Dim QueryString = CreateQueryString(query)
        Dim strToSign = String.Format("{0}&{1}&{2}",
                                      ToMethodString(method),
                                      UrlEncode(url), UrlEncode(QueryString))
        Dim key = String.Format("{0}&{1}",
                                UrlEncode(_consumerSecret),
                                UrlEncode(If(tokenSecret, String.Empty)))
        Using hmac = New HMACSHA1(Encoding.ASCII.GetBytes(key))
            Dim hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(strToSign))
            Return Convert.ToBase64String(hash)
        End Using
    End Function
#End Region


End Class
Module Utilities
    <Extension()> Function IsNullOrEmpty(ByVal value As String) As Boolean
        Return String.IsNullOrEmpty(value)
    End Function
    <Extension()> Function ToEnum(ByVal value As String) As HttpVerbs
        Return [Enum].Parse(GetType(HttpVerbs), value, True)
    End Function
End Module


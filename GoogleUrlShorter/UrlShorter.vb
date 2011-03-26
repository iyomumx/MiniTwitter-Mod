Imports System.Runtime.Serialization.Json
Imports System.Text.RegularExpressions

Public Class UrlShorter
    Inherits OAuthBase

    Public Const scope As String = "https://www.googleapis.com/auth/urlshortener"

    Public Sub New()
        Me.New("anonymous", "anonymous")
    End Sub

    Public Sub New(ByVal consumerKey As String, ByVal consumerSecret As String)
        MyBase.New(consumerKey, consumerSecret)
        initAcfun()
    End Sub

    Public Sub New(ByVal consumerKey As String, ByVal consumerSecret As String, ByVal token As String, ByVal tokenSecret As String)
        Me.New(consumerKey, consumerSecret)
        Me.WriteToken(token, tokenSecret)
    End Sub

    Sub initAcfun()
        Try
            acfun = (From ip In Net.Dns.GetHostAddresses("acfun.cn") Select value = ip.ToString).ToArray
        Catch ex As Exception
            acfun = Enumerable.Empty(Of String)().ToArray
        End Try
    End Sub

    Dim acfun() As String
    Private tmpToken As String = String.Empty

    Public Function ReadToken() As Tuple(Of String, String)
        Return Tuple.Create(_token, _tokenSecret)
    End Function

    Public Sub WriteToken(ByVal token As String, ByVal tokenSecret As String)
        _token = token
        _tokenSecret = tokenSecret
    End Sub

    Public Property Key As String
    Public Shared ReadOnly IPreg As New Regex("(?<head>https?://)(?<ip>(((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))\.){3}((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5])))/", RegexOptions.Compiled)

    Public Function ShortenUrl(ByVal LongUrl As String, Optional ByVal optFunc As Func(Of String, String) = Nothing) As String
        If IPreg.IsMatch(LongUrl) Then
            Dim ip = IPreg.Match(LongUrl).Groups("ip").Value
            If acfun.Contains(ip) Then
                LongUrl = IPreg.Replace(LongUrl, "${head}acfun.cn/")
            End If
        End If
        Dim o As Object = New With {.longUrl = LongUrl}
        Dim url = "https://www.googleapis.com/urlshortener/v1/url"
        If Not String.IsNullOrEmpty(Key) Then
            url += "?key=" + Key
        End If
        Dim r As ShortenReturn = Nothing
        Try
            r = Post(Of ShortenReturn)(url, o)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Return If(r IsNot Nothing, r.id, If(optFunc Is Nothing, LongUrl, optFunc(LongUrl)))
    End Function

    Public Function GetOriginalUrl(ByVal shortUrl As String) As String
        Return GetExpandReturn(shortUrl).longUrl
    End Function

    Public Function GetExpandReturn(ByVal shortUrl As String) As ExpandReutrn
        Dim url = "https://www.googleapis.com/urlshortener/v1/url"
        If Not String.IsNullOrEmpty(Key) Then
            url += "?key=" + Key
            url += "&shortUrl=" + shortUrl
        Else
            url += "?shortUrl=" + shortUrl
        End If
        Dim r As ExpandReutrn = Nothing
        Try
            r = [Get](Of ExpandReutrn)(url, Nothing)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Return If(r, New ExpandReutrn With {.id = shortUrl, .kind = "urlshortener#url", .longUrl = shortUrl, .status = "OK"})
    End Function
End Class

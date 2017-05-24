'   Copyright 2017 Artem Drobanov (artem.drobanov@gmail.com)

'   Licensed under the Apache License, Version 2.0 (the "License");
'   you may Not use this file except In compliance With the License.
'   You may obtain a copy Of the License at

'     http://www.apache.org/licenses/LICENSE-2.0

'   Unless required by applicable law Or agreed To In writing, software
'   distributed under the License Is distributed On an "AS IS" BASIS,
'   WITHOUT WARRANTIES Or CONDITIONS Of ANY KIND, either express Or implied.
'   See the License For the specific language governing permissions And
'   limitations under the License.

Imports System.IO
Imports System.Text
Imports System.Runtime.Serialization
Imports System.Security.Cryptography

Public Module StreamPassportManager
    Private _sha256 As New SHA256Cng()

    Public Function Create(id As String, stream As Stream) As StreamPassport
        stream.Seek(0, SeekOrigin.Begin)
        Dim sha256 = _sha256.ComputeHash(stream)
        Dim passport = New StreamPassport(id, sha256, stream.Length)
        Return passport
    End Function

    Public Function Load(stream As Stream) As StreamPassport
        Using sr = New StreamReader(stream)
            Dim objJson = sr.ReadToEnd()
            Dim obj = New StreamPassport()
            obj.Deserialize(objJson)
            With obj
                .ID = If(.ID Is Nothing, String.Empty, .ID)
                .SHA256 = If(.SHA256 Is Nothing, String.Empty, .SHA256)
                .StreamSize = If(.StreamSize Is Nothing, String.Empty, .StreamSize)
                .Total = If(.Total Is Nothing, String.Empty, .Total)
            End With
            Return obj
        End Using
    End Function
End Module

<DataContract>
Public Class StreamPassport
    Private _sha256Cng As New SHA256Cng()
    Private _sha256Man As New SHA256Managed()
    Private _sha256Csp As New SHA256CryptoServiceProvider()

    <DataMember>
    Public Property ID As String

    <DataMember>
    Public Property SHA256 As String

    <DataMember>
    Public Property StreamSize As String

    <DataMember>
    Public Property Total As String

    Public Sub New()
    End Sub

    Public Sub New(id As String, sha256 As Byte(), streamSize As Long)
        Me.ID = id
        Me.SHA256 = BytesToHex(sha256).ToUpper()
        Me.StreamSize = streamSize.ToString().ToUpper()
        Me.Total = CalcTotal().ToUpper()
    End Sub

    Public Function Serialize() As String
        Return Serializer.SaveObjectToJsonString(Me).Replace("{", "{" + vbCrLf).Replace("}", vbCrLf + "}").Replace(",", "," + vbCrLf)
    End Function

    Public Sub Deserialize(objJson As String)
        Dim obj = Serializer.LoadObjectFromJsonString(Of StreamPassport)(objJson)
        Me.ID = obj.ID
        Me.SHA256 = obj.SHA256.ToUpper()
        Me.StreamSize = obj.StreamSize.ToUpper()
        Me.Total = obj.Total.ToUpper()
    End Sub

    Public Function IsValid() As Boolean
        Return Me.Total = CalcTotal()
    End Function

    Public Function Compare(sprt As StreamPassport) As Boolean
        If Me.IsValid() AndAlso sprt.IsValid Then
            If Me.SHA256 = sprt.SHA256 AndAlso Me.StreamSize = sprt.StreamSize Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function

    Public Overrides Function ToString() As String
        Dim streamSizeLong = Long.Parse(Me.StreamSize)
        Return String.Format("| ID: {0,-64} | Size: {1, 26} | SHA-256: {2,-64} |", Me.ID.Substring(0, Math.Min(Me.ID.Length, 64)),
                             streamSizeLong.ToString("#,##0"), Me.SHA256)
    End Function

    Private Function CalcTotal() As String
        Dim hashBytes As New List(Of Byte)
        For Each b In Encoding.ASCII.GetBytes(Me.ID)
            hashBytes.Add(b)
        Next
        For Each b In Encoding.ASCII.GetBytes(Me.SHA256)
            hashBytes.Add(b)
        Next
        For Each b In Encoding.ASCII.GetBytes(Me.StreamSize)
            hashBytes.Add(b)
        Next
        Dim totalCng = BytesToHex(_sha256Cng.ComputeHash(hashBytes.ToArray()))
        Dim totalMan = BytesToHex(_sha256Man.ComputeHash(hashBytes.ToArray()))
        Dim totalCsp = BytesToHex(_sha256Csp.ComputeHash(hashBytes.ToArray()))
        If totalCng <> totalMan OrElse totalCng <> totalCsp Then
            Throw New Exception("StreamPassport: SHA-256 provider error!")
        Else
            Return totalCng
        End If
    End Function

    Private Function BytesToHex(data As Byte()) As String
        Return BitConverter.ToString(data).Replace("-", String.Empty).ToUpper()
    End Function
End Class

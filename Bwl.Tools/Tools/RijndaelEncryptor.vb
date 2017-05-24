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
Imports System.Security.Cryptography

Public NotInheritable Class RijndaelEncryptor
    Implements IDisposable
    Private Const _blockSize = 32 '256 bit
    Private _IV() As Byte
    Private _decryptor As ICryptoTransform
    Private _encryptor As ICryptoTransform
    Private _hash256 As SHA256Managed
    Private _hash512 As SHA512Managed
    Private _key() As Byte
    Private _rijndael As RijndaelManaged

    Public Shared Function Encode(inputData As Byte(), key() As Byte,
                                  Optional hashIterations As Integer = 1,
                                  Optional usePadding As Boolean = True) As Byte()
        If Not usePadding AndAlso inputData.Length Mod _blockSize <> 0 Then
            Throw New Exception("RijndaelEncryptor.Encode(): Not usePadding AndAlso inputData.Length Mod _blockSize <> 0")
        End If
        If Not (key IsNot Nothing AndAlso key.Length <> 0) Then
            key = New Byte() {0}
        End If
        Dim encryptor As New RijndaelEncryptor(key, hashIterations)
        Dim outputStream = New MemoryStream()
        Dim outputCryptoStream = encryptor.WrapStream(outputStream, True)
        Dim paddingLength = _blockSize - (inputData.Length Mod _blockSize)
        If paddingLength = _blockSize Then paddingLength = 0
        Dim padding = New Byte(paddingLength - 1) {}
        Dim rng As New RNGCryptoServiceProvider() : rng.GetBytes(padding)
        outputCryptoStream.Write(inputData, 0, inputData.Length)
        outputCryptoStream.Write(padding, 0, padding.Length)
        CType(outputCryptoStream, CryptoStream).FlushFinalBlock()
        If usePadding Then outputStream.WriteByte(((CByte(paddingLength) << 3) And &HF8) Or (DateTime.Now.Ticks And &H7))
        outputStream.Flush() : outputStream.Seek(0, SeekOrigin.Begin)
        Dim outputBlocksCount = CInt(Math.Ceiling(inputData.Length / _blockSize))
        Dim outputData = outputStream.GetBuffer() : Array.Resize(outputData, (outputBlocksCount * _blockSize) + If(usePadding, 1, 0))
        encryptor.Dispose()
        Return outputData
    End Function

    Public Shared Function Decode(inputData As Byte(), key() As Byte,
                                  Optional hashIterations As Integer = 1,
                                  Optional usePadding As Boolean = True) As Byte()
        If Not usePadding AndAlso inputData.Length Mod _blockSize <> 0 Then
            Throw New Exception("RijndaelEncryptor.Decode(): Not usePadding AndAlso inputData.Length Mod _blockSize <> 0")
        End If
        If usePadding AndAlso (inputData.Length - 1) Mod _blockSize <> 0 Then
            Throw New Exception("RijndaelEncryptor.Decode(): usePadding AndAlso inputData.Length - 1 Mod _blockSize <> 0")
        End If
        If Not (key IsNot Nothing AndAlso key.Length <> 0) Then
            key = New Byte() {0}
        End If
        Dim encryptor As New RijndaelEncryptor(key, hashIterations)
        Dim data = If(usePadding, New Byte(inputData.Length - 2) {}, New Byte(inputData.Length - 1) {})
        Array.Copy(inputData, data, data.Length)
        Dim outputStream = New MemoryStream(data)
        Dim inputStream = encryptor.WrapStream(outputStream, False)
        Dim dataStream As New MemoryStream()
        inputStream.CopyTo(dataStream)
        dataStream.Flush() : dataStream.Seek(0, SeekOrigin.Begin)
        Dim decBytesWithPadding = dataStream.GetBuffer() : Array.Resize(decBytesWithPadding, CInt(dataStream.Length))
        Dim paddingLength = If(usePadding, (CByte(inputData(inputData.Length - 1) >> 3)) And &H1F, 0)
        Dim decBytes = New Byte((decBytesWithPadding.Length - paddingLength) - 1) {}
        Array.Copy(decBytesWithPadding, decBytes, decBytes.Length)
        encryptor.Dispose()
        Return decBytes
    End Function

    Public Sub New()
    End Sub

    Public Sub New(password() As Byte, Optional hashIterations As Integer = 1)
        Initialize(password, hashIterations)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Clear() : GC.SuppressFinalize(Me)
    End Sub

    Private _isInitialized As Boolean
    Public Property IsInitialized() As Boolean
        Get
            Return _isInitialized
        End Get
        Private Set(value As Boolean)
            _isInitialized = value
        End Set
    End Property

    Private Sub Hash(data() As Byte, hashIterations As Integer, ByRef key() As Byte, ByRef IV() As Byte)
        Dim hash512Buff() As Byte = _hash512.ComputeHash(data)
        Dim hash256Buff() As Byte = _hash256.ComputeHash(data)
        For i = 1 To hashIterations - 1
            hash512Buff = _hash512.ComputeHash(hash512Buff)
            hash256Buff = _hash256.ComputeHash(hash256Buff)
        Next
        key = _hash256.ComputeHash(hash512Buff)
        IV = _hash256.ComputeHash(hash256Buff)
    End Sub

    Public Sub Initialize(password() As Byte, Optional hashIterations As Integer = 1)
        If hashIterations < 1 Then
            Throw New Exception("RijndaelEncryptor.Initialize(): hashIterations < 1")
        End If
        Clear()
        Hash(password, hashIterations, _key, _IV)
        _rijndael.Mode = CipherMode.CBC
        _rijndael.KeySize = (_key.Length << 3)
        _rijndael.BlockSize = _rijndael.KeySize
        _rijndael.Key = _key
        _rijndael.IV = _IV
        _encryptor = _rijndael.CreateEncryptor()
        _decryptor = _rijndael.CreateDecryptor()
        IsInitialized = True
    End Sub

    Public Sub Clear()
        IsInitialized = False
        ClearArray(_key)
        ClearArray(_IV)
        If _hash256 IsNot Nothing Then _hash256.Clear()
        If _hash512 IsNot Nothing Then _hash512.Clear()
        If _rijndael IsNot Nothing Then _rijndael.Clear()
        _hash256 = New SHA256Managed()
        _hash512 = New SHA512Managed()
        _rijndael = New RijndaelManaged() With {.Padding = PaddingMode.None}
    End Sub

    Public Function WrapStream(stream As Stream, encryptionMode As Boolean) As Stream
        If Not IsInitialized Then
            Throw New Exception("RijndaelEncryptor is not initialized!")
        End If
        Return If(encryptionMode, New CryptoStream(stream, _encryptor, CryptoStreamMode.Write), New CryptoStream(stream, _decryptor, CryptoStreamMode.Read))
    End Function

    Private Shared Sub ClearArray(Of T)(array() As T)
        If array Is Nothing Then Return
        System.Array.Clear(array, 0, array.Length)
    End Sub
End Class

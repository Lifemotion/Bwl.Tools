Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class CryptoToolsTest

    <TestMethod()>
    Public Sub TestCryptography()
        Try
            Dim tdes = new TripleDESCryptoServiceProvider()
            tdes.GenerateIV()
            tdes.GenerateKey()

            Dim key = tdes.Key()
            Dim value = "This is a string for encoding"

            tdes.Dispose()
            GC.Collect()

            Dim result1 = CryptoTools.Des3Encode(value,key)
            Dim result2 = CryptoTools.Des3EncodeB(value, key)
            Dim result3 = CryptoTools.Des3Decode(result1,key)
            Dim result4 = CryptoTools.Des3DecodeB(result2,key)

            Assert.AreEqual(result3, result4)
            Assert.AreEqual(result3, value)
            Assert.AreEqual(result4, value)
        Catch ex As Exception
            Throw
        End Try
        
    End Sub
    
End Class
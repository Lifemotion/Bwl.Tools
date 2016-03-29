Imports System.Runtime.Remoting.Messaging
Imports System.Runtime.Serialization
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass> Public Class SerializerTest

    <TestMethod()> Public Sub SerializerTest()
        Dim data As New ExampleClass(16,"example")
        Try
            Dim result1 = Serializer.SaveObjectToJsonBytes(data)
            Dim result2 = Serializer.SaveObjectToJsonString(data)
            Dim result3 = Serializer.LoadObjectFromJsonBytes(Of ExampleClass)(result1)
            Dim result4 = Serializer.LoadObjectFromJsonString(Of ExampleClass)(result2)

            Assert.AreEqual(result3.Number, result4.Number)
            Assert.AreEqual(result3.text, result4.text)

            Assert.AreEqual(result3.Number, data.Number)
            Assert.AreEqual(result3.text, data.text)

            Assert.AreEqual(result4.Number, data.Number)
            Assert.AreEqual(result4.text, data.text)
        Catch ex As Exception
            Throw
        End Try
    End Sub
    
End Class


<DataContract>
Public Class ExampleClass
    <DataMember>
    Public Number As Integer
    <DataMember>
    Public Text As String
    Public Sub New(n As Integer, s As String)
        number = n
        text = s
    End Sub
End Class
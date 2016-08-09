Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FieldMonitorTest
    Public Class TestClass
        Public Property Test1 As String = "1234"
        Public Property Test2 As String
        Public Property Test3 As Integer
        Public Property Test4 As Single = 0.1
        Public Property Test5 As Double
        Public Property Test6 As Boolean = True
    End Class

    <TestMethod()>
    Public Sub TestFieldMonitorTest()
        Dim mark1 As Integer
        Dim mark2 As Integer
        Dim mark3 As Integer
        Dim mark4 As Integer
        Dim mark5 As Integer
        Dim mark6 As Integer
        Dim testclass As New TestClass
        Dim monitor As New FieldMonitor(testclass, "Test")
        AddHandler monitor.FieldChangedDetailed, Sub(target As Object, field As PropertyInfo, fieldName As String, lastValue As Object, currentValue As Object)
                                                     If fieldName = "Test1" Then mark1 += 1
                                                     If fieldName = "Test2" Then mark2 += 1
                                                     If fieldName = "Test3" Then mark3 += 1
                                                     If fieldName = "Test4" Then mark4 += 1
                                                     If fieldName = "Test5" Then mark5 += 1
                                                     If fieldName = "Test6" Then mark6 += 1 = True
                                                 End Sub
        testclass.Test1 = "34346436"
        testclass.Test2 = "443563"
        testclass.Test3 = -125
        testclass.Test4 = 0.6
        testclass.Test5 = -0.3424
        testclass.Test6 = False
        Threading.Thread.Sleep(1000)
        Assert.AreEqual(1, mark1)
        Assert.AreEqual(1, mark2)
        Assert.AreEqual(1, mark3)
        Assert.AreEqual(1, mark4)
        Assert.AreEqual(1, mark5)
        Assert.AreEqual(1, mark6)
    End Sub

    Private Sub testclass1()
        Throw New NotImplementedException()
    End Sub
End Class

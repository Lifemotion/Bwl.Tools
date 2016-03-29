Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class StopwatchTest

    <TestMethod()>
    Public Sub Stopwatch()
        Dim sw As New Stopwatch
        Dim result1 = sw.FinishAndStart
        Threading.Thread.Sleep(50)
        Dim result2 = sw.FinishAndStart
        Threading.Thread.Sleep(110)
        Dim result3 = sw.FinishAndStart

        Assert.IsTrue(result1.TotalMilliseconds < 2)

        Assert.IsTrue(result2.TotalMilliseconds < 55)
        Assert.IsTrue(result2.TotalMilliseconds > 45)

        Assert.IsTrue(result3.TotalMilliseconds < 115)
        Assert.IsTrue(result3.TotalMilliseconds > 105)

        'If result1.TotalMilliseconds > 2 Then Throw New Exception("Fail, result 1 must be 0 ms, real is " + result1.TotalMilliseconds.ToString)

        'If result2.TotalMilliseconds > 55 Then Throw New Exception("Fail, result 2 must be 50 ms, real is " + result2.TotalMilliseconds.ToString)
        'If result2.TotalMilliseconds < 45 Then Throw New Exception("Fail, result 2 must be 50 ms, real is " + result2.TotalMilliseconds.ToString)

        'If result3.TotalMilliseconds > 115 Then Throw New Exception("Fail, result 3 must be 110 ms, real is " + result3.TotalMilliseconds.ToString)
        'If result3.TotalMilliseconds < 105 Then Throw New Exception("Fail, result 3 must be 110 ms, real is " + result3.TotalMilliseconds.ToString)
    End Sub
End Class
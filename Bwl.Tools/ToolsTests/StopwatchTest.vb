Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class StopwatchTest

    <TestMethod()> Public Sub Stopwatch()
        Dim sw As New Stopwatch
        Dim result1 = sw.FinishAndStart
        Threading.Thread.Sleep(50)
        Dim result2 = sw.FinishAndStart
        Threading.Thread.Sleep(110)
        Dim result3 = sw.FinishAndStart

        If result1.TotalMilliseconds > 2 Then Throw New Exception("Fail, must be 0 ms, real is " + result1.TotalMilliseconds.ToString)

        If result2.TotalMilliseconds > 55 Then Throw New Exception("Fail, must be 50 ms, real is " + result2.TotalMilliseconds.ToString)
        If result2.TotalMilliseconds < 45 Then Throw New Exception("Fail, must be 50 ms, real is " + result2.TotalMilliseconds.ToString)

        If result3.TotalMilliseconds > 115 Then Throw New Exception("Fail, must be 110 ms, real is " + result3.TotalMilliseconds.ToString)
        If result3.TotalMilliseconds < 105 Then Throw New Exception("Fail, must be 110 ms, real is " + result3.TotalMilliseconds.ToString)
    End Sub
End Class
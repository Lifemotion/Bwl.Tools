Imports System.IO
Imports System.Threading
Imports System.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RunLimiterTest
    <TestMethod()>
    Public Sub RunLimiterTest1()
        Dim periodMs = 100
        Dim runLimiter As New RunLimiter(periodMs)
        Dim activityTimes As New List(Of DateTime)
        Dim activityFlags As New List(Of Boolean)
        Dim sw As New Diagnostics.Stopwatch()
        sw.Start()
        For i = 1 To 1000
            Dim res = runLimiter.Run(Sub() activityTimes.Add(Now))
            activityFlags.Add(res)
            Thread.Sleep(TimeSpan.FromMilliseconds(periodMs / 10))
        Next
        sw.Stop()
        Assert.AreEqual(activityTimes.Count, activityFlags.Where(Function(item) item).Count) 'Количество положительных флагов должно соотв. отработкам
        Dim experimentTimeMs = sw.Elapsed.TotalMilliseconds
        Dim activitiesMustBe = Math.Floor(experimentTimeMs / periodMs)
        Dim deltaPerc = Math.Abs(activityTimes.Count - activitiesMustBe) / activitiesMustBe 'Реальная отработка не должна сильно отличаться от расчетной
        Assert.IsTrue(deltaPerc < 0.1)
    End Sub

    <TestMethod()>
    Public Sub RunLimiterTest2()
        Dim runLimiter As New RunLimiter()
        Try
            runLimiter.Run(Sub()
                               Throw New Exception("RunLimiterTest")
                           End Sub)
        Catch ex As Exception
        End Try
    End Sub
End Class

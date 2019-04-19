Imports System.IO
Imports System.Threading
Imports System.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RunLimiterTest
    <TestMethod()>
    Public Sub RunLimiterTest()
        Dim limitSeconds = 0.1
        Dim runLimiter As New RunLimiter(limitSeconds)
        Dim activityTimes As New List(Of DateTime)
        Dim activityFlags As New List(Of Boolean)
        Dim sw As New Diagnostics.Stopwatch()
        sw.Start()
        For i = 1 To 1000
            Dim res = runLimiter.Run(Sub() activityTimes.Add(Now))
            activityFlags.Add(res)
            Thread.Sleep(TimeSpan.FromSeconds(limitSeconds / 10))
        Next
        sw.Stop()
        Assert.AreEqual(activityTimes.Count, activityFlags.Where(Function(item) item).Count) 'Количество положительных флагов должно соотв. отработкам
        Dim experimentTimeSeconds = sw.Elapsed.TotalSeconds
        Dim activitiesMustBe = Math.Floor(experimentTimeSeconds / limitSeconds)
        Dim deltaPerc = Math.Abs(activityTimes.Count - activitiesMustBe) / activitiesMustBe 'Реальная отработка не должна сильно отличаться от расчетной
        Assert.IsTrue(deltaPerc < 0.1)
    End Sub
End Class

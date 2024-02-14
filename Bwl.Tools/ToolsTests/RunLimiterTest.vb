Imports System.Threading
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RunLimiterTest
    <TestMethod()>
    Public Sub RunLimiterActivityTest()
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
    Public Sub RunLimiterMemoryCleanTest()
        Dim nRuns = 1000
        Dim runLimiter1 As New RunLimiter(periodMs:=1, memoryDepthPeriods:=0, memorySizeMax:=0, memoryCleanPeriodMs:=0)
        Dim runLimiter2 As New RunLimiter(periodMs:=1, memoryDepthPeriods:=2, memorySizeMax:=nRuns, memoryCleanPeriodMs:=10000)
        For i = 1 To nRuns
            runLimiter1.Run(Sub()
                            End Sub, Guid.NewGuid.ToString("B"))
            runLimiter2.Run(Sub()
                            End Sub, Guid.NewGuid.ToString("B"))
            Thread.Sleep(1)
        Next
        Assert.IsTrue(runLimiter1.Count = 1)
        Assert.IsTrue(runLimiter2.Count = nRuns)
    End Sub

    <TestMethod()>
    Public Sub RunLimiterSuppressExceptionsTest()
        For Each suppressExceptions In {False, True}
            Dim exceptionDetected = False
            Dim exceptionExpected = Not suppressExceptions
            Dim runLimiter As New RunLimiter()
            Try
                runLimiter.Run(Sub()
                                   Throw New Exception("RunLimiterTest")
                               End Sub, suppressExceptions:=suppressExceptions)
            Catch ex As Exception
                exceptionDetected = True
            End Try
            Assert.IsTrue(exceptionDetected = exceptionExpected)
        Next
    End Sub
End Class

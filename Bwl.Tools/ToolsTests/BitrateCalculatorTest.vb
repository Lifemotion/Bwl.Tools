Imports System.IO
Imports System.Threading
Imports System.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class BitrateCalculatorTest
    <TestMethod()>
    Public Sub BitrateCalculatorTest1()
        Dim bitrateCalculators As New BitrateCalculators()
        Dim bitrateIds = {"1Mbps", "2Mbps", "3Mbps", "4Mbps", "5Mbps"}
        Dim mbps = 1000 * 1000 / 8
        Dim bitrateData = {1 * mbps, 2 * mbps, 3 * mbps, 4 * mbps, 5 * mbps}
        bitrateCalculators.ResetAll()
        Dim N = 10
        For i = 1 To N
            For id = 0 To bitrateIds.Length - 1
                bitrateCalculators.Update(bitrateData(id), bitrateIds(id))
            Next
            Thread.Sleep(1000 / N)
        Next
        Dim calculatedBitrates = bitrateCalculators.Bitrates.Select(Function(item) item.Value).ToArray()
        For id = 0 To bitrateIds.Length - 1
            Assert.IsTrue(Math.Abs(calculatedBitrates(id) * mbps - bitrateData(id)) / bitrateData(id) < 0.05)
        Next
    End Sub
End Class

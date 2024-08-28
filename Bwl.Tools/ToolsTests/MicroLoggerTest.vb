Imports System.IO
Imports System.Threading
Imports System.Collections.Concurrent
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class MicroLoggerTest
    <TestMethod()>
    Public Sub MicroLoggerTest()
        Dim stringCount = 10000
        Dim writerCount = 100
        Dim logPath = String.Empty
        Dim logFileName = "MicroLoggerTest.log"

        Dim cleaningAction = Sub()
                                 Try
                                     File.Delete(Path.Combine(logPath, logFileName))
                                 Catch
                                 End Try
                             End Sub

        For Each updateDelayMs In {10, 100, 1000}
            'Clean
            cleaningAction()

            'Write
            Using logger = New MicroLogger(logPath, logFileName, updateDelayMs)
                Dim sourceLines As New ConcurrentQueue(Of String)
                For i = 0 To stringCount - 1
                    sourceLines.Enqueue(i)
                Next
                Dim threads As New Queue(Of Thread)()
                For i = 0 To writerCount - 1
                    Dim thr = New Thread(Sub()
                                             For Each line In sourceLines
                                                 logger.AddMessage(line, "msg")
                                             Next
                                         End Sub) With {.IsBackground = True}
                    threads.Enqueue(thr)
                Next
                threads.AsParallel().ForAll(Sub(thr As Thread) thr.Start())
                threads.AsParallel().ForAll(Sub(thr As Thread) thr.Join())
            End Using

            'Read
            Dim targetLinesDic As New Dictionary(Of String, Integer)
            For Each line In File.ReadAllLines(Path.Combine(logPath, logFileName))
                line = line.Split(" ").Last()
                If Not targetLinesDic.ContainsKey(line) Then
                    targetLinesDic.Add(line, 0)
                End If
                targetLinesDic(line) += 1
            Next
            For Each targetLineKVP In targetLinesDic
                If targetLineKVP.Value <> writerCount Then
                    Throw New Exception("targetLineKVP.Value <> writerCount")
                End If
            Next

            'Clean
            cleaningAction()
        Next
    End Sub
End Class

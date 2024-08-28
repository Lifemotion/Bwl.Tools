Imports System.IO
Imports System.Threading
Imports System.Collections.Concurrent

Public Class MicroLogger
    Implements IDisposable

    Private _lines As New ConcurrentQueue(Of String)()
    Private _cts As New CancellationTokenSource()
    Private _task As Task

    Public Property Path As String
    Public Property FileName As String
    Public Property UpdateDelayMs As Integer

    Public Event OnException As EventHandler(Of Exception)

    Public Sub New(path As String, fileName As String, Optional updateDelayMs As Integer = 1000)
        Me.Path = path
        Me.FileName = fileName
        Me.UpdateDelayMs = updateDelayMs
        _task = Task.Run(Sub() WriteTask(_cts.Token), _cts.Token)
    End Sub

    Public Sub AddMessage(message As String, Optional messageType As String = "")
        Dim messageTypeMarker = If(Not String.IsNullOrWhiteSpace(messageType), $" [{messageType}] ", "")
        _lines.Enqueue($"{DateTime.Now.ToString("<dd.MM.yyyy HH:mm:ss.fff>")}{messageTypeMarker}{message}")
    End Sub

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Dim task = Interlocked.Exchange(_task, Nothing)
        If task IsNot Nothing Then
            _cts.Cancel()
            task.Wait()
            _cts.Dispose()
        End If
    End Sub

    Private Sub WriteTask(ct As CancellationToken)
        Do While Not ct.IsCancellationRequested OrElse _lines.Any()
            Try
                If _lines.Any() Then
                    Dim pf = (Me.Path, Me.FileName)
                    If Not String.IsNullOrWhiteSpace(pf.Path) AndAlso Not Directory.Exists(pf.Path) Then
                        Directory.CreateDirectory(pf.Path)
                    End If
                    Using sw = File.AppendText(IO.Path.Combine(pf.Path, pf.FileName))
                        Dim line As String = Nothing
                        While _lines.TryDequeue(line)
                            sw.WriteLine(line)
                        End While
                    End Using
                End If
            Catch ex As Exception
                RaiseEvent OnException(Me, ex)
            Finally
                Thread.Sleep(UpdateDelayMs)
            End Try
        Loop
    End Sub
End Class

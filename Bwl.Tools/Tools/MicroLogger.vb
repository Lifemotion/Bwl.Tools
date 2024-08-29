Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Concurrent

Public Class MicroLogger
    Implements IDisposable

    Private _linesToWrite As New ConcurrentQueue(Of String)()
    Private _loggerTask As Task
    Private _disposeUtcTicks As Long = -1

    Public Property Path As String
    Public Property FileName As String
    Public Property UpdateDelayMs As Integer
    Public Property UnsavedAwaitMs As Integer

    Public Event OnException As EventHandler(Of Exception)

    Public Sub New(path As String, fileName As String,
                   Optional updateDelayMs As Integer = 1000,
                   Optional unsavedAwaitMs As Integer = 5000)
        Me.Path = path
        Me.FileName = fileName
        Me.UpdateDelayMs = updateDelayMs
        Me.UnsavedAwaitMs = unsavedAwaitMs
        _loggerTask = Task.Run(AddressOf LoggerTask)
    End Sub

    Public Sub AddMessage(message As String, Optional messageType As String = "")
        Dim messageTypeMarker = If(Not String.IsNullOrWhiteSpace(messageType), $" [{messageType}] ", "")
        _linesToWrite.Enqueue($"{DateTime.Now.ToString("<dd.MM.yyyy HH:mm:ss.fff>")}{messageTypeMarker}{message}")
    End Sub

    Private Sub LoggerTask()
        While DisposeRequested() Is Nothing OrElse (_linesToWrite.Any() AndAlso DisposeRequested().Value <= UnsavedAwaitMs)
            Try
                If _linesToWrite.Any() Then
                    Dim pf = (Me.Path, Me.FileName)
                    If Not String.IsNullOrWhiteSpace(pf.Path) AndAlso Not Directory.Exists(pf.Path) Then
                        Directory.CreateDirectory(pf.Path)
                    End If
                    Using sw = File.AppendText(IO.Path.Combine(pf.Path, pf.FileName))
                        Dim line As String = Nothing
                        While _linesToWrite.TryDequeue(line)
                            sw.WriteLine(line)
                        End While
                    End Using
                End If
            Catch ex As Exception
                RaiseEvent OnException(Me, ex)
            Finally
                Thread.Sleep(UpdateDelayMs)
            End Try
        End While
        If _linesToWrite.Any() Then
            RaiseEvent OnException(Me, New Exception($"Unsaved lines"))
        End If
    End Sub

    Private Function DisposeRequested() As Double?
        Dim disposeUtcTicks = Interlocked.Read(_disposeUtcTicks)
        Return If(disposeUtcTicks < 0, DirectCast(Nothing, Double?), TimeSpan.FromTicks(DateTime.UtcNow.Ticks - disposeUtcTicks).TotalMilliseconds)
    End Function

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Dim task = Interlocked.Exchange(_loggerTask, Nothing)
        If task IsNot Nothing Then
            Interlocked.Exchange(_disposeUtcTicks, DateTime.UtcNow.Ticks)
            task.Wait()
        End If
    End Sub
End Class

Imports System.Reflection

Public Class FieldMonitor
    Implements IDisposable
    Public Event FieldChanged(target As Object, field As PropertyInfo)
    Public Event FieldChangedDetailed(target As Object, field As PropertyInfo, fieldName As String, lastValue As Object, currentValue As Object)

    Private _items As New List(Of FieldMonitorItem)
    Private _thread As Threading.Thread
    Private _monitorDelayMs As Integer

    Public Class FieldMonitorItem
        Public Property LastFieldValue As Object
        Public Property Target As Object
        Public Property PropertyInfo As PropertyInfo
    End Class

    Public Sub New(target As Object, Optional monitorDelayMs As Integer = 500, Optional filterByName As String = "Setting")
        _monitorDelayMs = monitorDelayMs
        For Each prop In target.GetType.GetProperties
            If filterByName = "" OrElse prop.Name.Contains(filterByName) Then
                Select Case prop.PropertyType
                    Case GetType(String), GetType(Integer), GetType(Double), GetType(Single), GetType(Boolean)
                        Dim val = prop.GetValue(target, Nothing)
                        Dim info As New FieldMonitorItem
                        info.LastFieldValue = val
                        info.PropertyInfo = prop
                        info.Target = target
                        _items.Add(info)
                End Select
            End If
        Next

        If _items.Count > 0 Then
            _thread = New Threading.Thread(AddressOf MonitorThread)
            _thread.Name = "FieldMonitor Worker"
            _thread.IsBackground = True
            _thread.Start()
        End If
    End Sub

    Public ReadOnly Property MonitoredFields As FieldMonitorItem()
        Get
            Return _items.ToArray
        End Get
    End Property

    Private Sub MonitorThread()
        Do
            Try
                For Each item In _items
                    Try

                        Dim fieldVal = item.PropertyInfo.GetValue(item.Target, Nothing)
                        If fieldVal <> item.LastFieldValue Then
                            RaiseEvent FieldChanged(item.Target, item.PropertyInfo)
                            RaiseEvent FieldChangedDetailed(item.Target, item.PropertyInfo, item.PropertyInfo.Name, item.LastFieldValue, fieldVal)
                        End If
                        item.LastFieldValue = fieldVal
                    Catch ex As Exception
                    End Try
                Next
            Catch ex As Exception
            End Try
            Threading.Thread.Sleep(_monitorDelayMs)
        Loop
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' Для определения избыточных вызовов

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                Try
                    _thread.Abort()
                Catch ex As Exception
                End Try

                Try
                    _items.Clear()
                Catch ex As Exception
                End Try
                _items = Nothing
                _thread = Nothing
            End If
        End If
        disposedValue = True
    End Sub

    ' Этот код добавлен редактором Visual Basic для правильной реализации шаблона высвобождаемого класса.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Не изменяйте этот код. Разместите код очистки выше в методе Dispose(disposing As Boolean).
        Dispose(True)
        ' TODO: раскомментировать следующую строку, если Finalize() переопределен выше.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class


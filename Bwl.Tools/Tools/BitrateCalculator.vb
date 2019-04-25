Imports System.Threading

Public Class BitrateCalculator
    Private _dataCounts As New Queue(Of Integer)
    Private _sw As New System.Diagnostics.Stopwatch

    Private _syncRoot As New Object

    Public Property MaxHistorySize As Integer = 100

    Public ReadOnly Property MegaBitsPerSecond As Double
        Get
            SyncLock _syncRoot
                If _dataCounts.Count < 2 Then Return -1
                Return (_dataCounts.Average() * 8) / (_sw.Elapsed.TotalSeconds * 1000 * 1000) 'Сетевой мегабит - степень десятки
            End SyncLock
        End Get
    End Property

    Public Sub New()
        Reset()
    End Sub

    Public Sub Update(data As IEnumerable(Of Byte))
        Update(data.Count)
    End Sub

    Public Sub Update(dataCount As Integer)
        SyncLock _syncRoot
            _dataCounts.Enqueue(dataCount)
            If _dataCounts.Count > MaxHistorySize Then
                _dataCounts.Dequeue()
            End If
        End SyncLock
    End Sub

    Public Sub Reset()
        SyncLock _syncRoot
            With _sw
                .Reset()
                .Start()
            End With
            _dataCounts.Clear()
        End SyncLock
    End Sub
End Class

Public Class BitrateCalculators
    Private _bitrates As New Dictionary(Of String, BitrateCalculator)
    Private _syncRoot As New Object

    Public ReadOnly Property Bitrates As IEnumerable(Of KeyValuePair(Of String, Double))
        Get
            SyncLock _syncRoot
                Return _bitrates.Select(Function(kvp) New KeyValuePair(Of String, Double)(kvp.Key, kvp.Value.MegaBitsPerSecond))
            End SyncLock
        End Get
    End Property

    Public Sub Update(dataCount As Integer, Optional bitrateId As String = "noname")
        SyncLock _syncRoot
            If Not _bitrates.ContainsKey(bitrateId) Then
                _bitrates.Add(bitrateId, New BitrateCalculator())
            End If
            _bitrates(bitrateId).Update(dataCount)
        End SyncLock
    End Sub

    Public Sub Reset(Optional bitrateId As String = "noname")
        SyncLock _syncRoot
            If _bitrates.ContainsKey(bitrateId) Then
                _bitrates(bitrateId).Reset()
            End If
        End SyncLock
    End Sub

    Public Sub ResetAll()
        SyncLock _syncRoot
            For Each bitrate In _bitrates
                Reset(bitrate.Key)
            Next
        End SyncLock
    End Sub
End Class

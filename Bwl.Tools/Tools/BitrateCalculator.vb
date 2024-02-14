'   Copyright 2019 Artem Drobanov (artem.drobanov@gmail.com)

'   Licensed under the Apache License, Version 2.0 (the "License");
'   you may Not use this file except In compliance With the License.
'   You may obtain a copy Of the License at

'     http://www.apache.org/licenses/LICENSE-2.0

'   Unless required by applicable law Or agreed To In writing, software
'   distributed under the License Is distributed On an "AS IS" BASIS,
'   WITHOUT WARRANTIES Or CONDITIONS Of ANY KIND, either express Or implied.
'   See the License For the specific language governing permissions And
'   limitations under the License.

Imports System.Linq
Imports System.Threading
Imports System.Collections.Generic

Public Class BitrateCalculator
    Private _dataCounts As New Queue(Of KeyValuePair(Of DateTime, Integer))
    Private _syncRoot As New Object

    Public Property MaxHistorySize As Integer = 100

    Public ReadOnly Property MegaBitsPerSecond As Double
        Get
            SyncLock _syncRoot
                If _dataCounts.Count < 2 Then Return -1
                Dim timeDelta = _dataCounts.Last().Key - _dataCounts.First().Key
                Dim dataValues = _dataCounts.Select(Function(item) item.Value)
                Dim dataSum = dataValues.Sum() - dataValues.First
                Return (dataSum * 8) / (timeDelta.TotalSeconds * 1000 * 1000) 'Сетевой мегабит - степень десятки
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
            _dataCounts.Enqueue(New KeyValuePair(Of Date, Integer)(Now, dataCount))
            While _dataCounts.Count > MaxHistorySize
                _dataCounts.Dequeue()
            End While
        End SyncLock
    End Sub

    Public Sub Reset()
        SyncLock _syncRoot
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

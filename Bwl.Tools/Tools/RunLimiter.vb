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

''' <summary>
''' Ограничитель частоты вызовов какого-либо кода, например, записи в лог.
''' </summary>
Public Class RunLimiter
    Private _limitSeconds As Double
    Private _runs As New Dictionary(Of String, DateTime)

    Sub New(Optional limitSeconds As Double = 1)
        _limitSeconds = limitSeconds
    End Sub

    Public Function Run(action As Action, Optional actionId As String = "noname") As Boolean
        Dim res As Boolean = False
        If Not _runs.ContainsKey(actionId) Then
            _runs.Add(actionId, DateTime.MinValue)
        End If
        If (Now - _runs(actionId)).TotalSeconds >= _limitSeconds Then
            action()
            _runs(actionId) = Now
            res = True
        End If
        Return res
    End Function
End Class

Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RijndaelEncryptorTest

    <TestMethod>
    Public Sub RijndaelEncryptorTest()
        Dim rnd As New Random(Now.Ticks Mod Integer.MaxValue)
        'С "ручным" выравниванием (можно подавать данные любой длины)
        For hashIterations = 1 To 4
            For dataByteCount = 1 To 100
                Dim data = New Byte(dataByteCount - 1) {} : rnd.NextBytes(data)
                For keyByteCount = 1 To 100
                    Dim key = New Byte(keyByteCount - 1) {} : rnd.NextBytes(key) : data(0) = data(0) Or 1 'Один байт должен быть точно не ноль (для возможности инверсии ключа)!
                    Dim usePadding = True 'Шифрование с выравниванием (данные любой длины), добавляется 1 байт к потоку кроме самого выравнивания
                    Dim enc = RijndaelEncryptor.Encode(data, key, hashIterations, usePadding)
                    Dim dec = RijndaelEncryptor.Decode(enc, key, hashIterations, usePadding) 'Нормальное дешифрование...
                    For check = 0 To data.Length - 1
                        If data(check) <> dec(check) Then
                            Throw New Exception("data(check) <> dec(check)")
                        End If
                    Next
                    For i = 0 To key.Length - 1 '...и с новым ключом (должно быть с ошибкой)
                        key(i) = key(i) Xor 255
                    Next
                    Dim paddingErrorDetected = False
                    Try
                        RijndaelEncryptor.Decode(enc, key, hashIterations, usePadding) 'Должно быть без ошибки, несмотря на некорректный ключ!
                    Catch
                        paddingErrorDetected = True
                    End Try
                    If paddingErrorDetected Then
                        Throw New Exception("Padding error detected, but should be not")
                    End If
                Next
            Next
        Next

        'Без "ручного" выравнивания (можно подавать данные, кратные размеру ключа)
        For hashIterations = 1 To 4
            For dataByteCount = 32 To 3200 Step 32
                Dim data = New Byte(dataByteCount - 1) {} : rnd.NextBytes(data)
                For keyByteCount = 1 To 100
                    Dim key = New Byte(keyByteCount - 1) {} : rnd.NextBytes(key) : data(0) = data(0) Or 1 'Один байт должен быть точно не ноль (для возможности инверсии ключа)!
                    Dim notUsePadding = False 'Шифрование без выравнивания (ошибки расшифровки НЕ обнаруживаются, но данные всегда кратны 32 байтам)
                    Dim enc = RijndaelEncryptor.Encode(data, key, hashIterations, notUsePadding)
                    Dim dec = RijndaelEncryptor.Decode(enc, key, hashIterations, notUsePadding) 'Нормальное дешифрование...
                    For check = 0 To data.Length - 1
                        If data(check) <> dec(check) Then
                            Throw New Exception("data(check) <> dec(check)")
                        End If
                    Next
                    For i = 0 To key.Length - 1 '...и с новым ключом (должно быть с необнаруживаемой ошибкой)
                        key(i) = key(i) Xor 255
                    Next
                    Dim paddingErrorDetected = False
                    Try
                        RijndaelEncryptor.Decode(enc, key, hashIterations, notUsePadding) 'Должно быть без ошибки, несмотря на некорректный ключ!
                    Catch
                        paddingErrorDetected = True
                    End Try
                    If paddingErrorDetected Then
                        Throw New Exception("Padding error detected, but should be not")
                    End If
                Next
            Next
        Next
    End Sub
End Class

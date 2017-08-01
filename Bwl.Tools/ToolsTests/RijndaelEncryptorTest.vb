Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RijndaelEncryptorTest

    <TestMethod>
    Public Sub RijndaelEncryptorTest()
        Dim rnd As New Random(Now.Ticks Mod Integer.MaxValue)
        Parallel.For(0, 4, Sub(hashIterations As Integer)
                               For dataByteCount = 1 To 63 'С "ручным" выравниванием (можно подавать данные любой длины)
                                   Dim data = New Byte(dataByteCount - 1) {} : rnd.NextBytes(data)
                                   For keyByteCount = 1 To 63
                                       Dim key = New Byte(keyByteCount - 1) {} : rnd.NextBytes(key) : data(0) = data(0) Or 1 'Один байт должен быть точно не ноль (для возможности инверсии ключа)!
                                       Dim usePadding = True 'Шифрование с выравниванием (данные любой длины), добавляется 1 байт к потоку кроме самого выравнивания
                                       Dim enc = If(hashIterations = 0,
                                                    RijndaelEncryptor.Encode(data, key, 100, usePadding), '100 итераций хеша
                                                    RijndaelEncryptor.Encode(data, key, hashIterations, usePadding))
                                       Dim dec = If(hashIterations = 0,
                                                    RijndaelEncryptor.Decode(enc, key, 100, usePadding), '100 итераций хеша
                                                    RijndaelEncryptor.Decode(enc, key, hashIterations, usePadding))  'Нормальное дешифрование...
                                       For check = 0 To data.Length - 1
                                           If data(check) <> dec(check) Then
                                               Throw New Exception("data(check) <> dec(check)")
                                           End If
                                       Next
                                       For i = 0 To key.Length - 1 '...и с новым ключом (должно быть с ошибкой)
                                           key(i) = key(i) Xor 255 'инвертируем ключ
                                       Next
                                       Dim paddingErrorDetected = False
                                       Try
                                           'Должно быть без ошибки, несмотря на некорректный ключ (используем невыровненные данные, но выравниваем внутри,
                                           'и внутренняя обработка без выравнивания)
                                           Dim dec2 = If(hashIterations = 0,
                                                         RijndaelEncryptor.Decode(enc, key, 100, usePadding), '100 итераций хеша
                                                         RijndaelEncryptor.Decode(enc, key, hashIterations, usePadding))
                                       Catch
                                           paddingErrorDetected = True
                                       End Try
                                       If paddingErrorDetected Then
                                           Throw New Exception("Padding error detected, but should be not")
                                       End If
                                   Next
                               Next

                               For dataByteCount = 32 To 1024 Step 32 'Без "ручного" выравнивания (можно подавать данные, кратные размеру ключа)
                                   Dim data = New Byte(dataByteCount - 1) {} : rnd.NextBytes(data)
                                   For keyByteCount = 1 To 63
                                       Dim key = New Byte(keyByteCount - 1) {} : rnd.NextBytes(key) : data(0) = data(0) Or 1 'Один байт должен быть точно не ноль (для возможности инверсии ключа)!
                                       Dim notUsePadding = False 'Шифрование без выравнивания (ошибки расшифровки НЕ обнаруживаются, но данные всегда кратны 32 байтам)
                                       Dim enc = If(hashIterations = 0,
                                                    RijndaelEncryptor.Encode(data, key, 100, notUsePadding), '100 итераций хеша
                                                    RijndaelEncryptor.Encode(data, key, hashIterations, notUsePadding))
                                       Dim dec = If(hashIterations = 0,
                                                    RijndaelEncryptor.Decode(enc, key, 100, notUsePadding), '100 итераций хеша
                                                    RijndaelEncryptor.Decode(enc, key, hashIterations, notUsePadding))  'Нормальное дешифрование...
                                       For check = 0 To data.Length - 1
                                           If data(check) <> dec(check) Then
                                               Throw New Exception("data(check) <> dec(check)")
                                           End If
                                       Next
                                       For i = 0 To key.Length - 1 '...и с новым ключом (должно быть с необнаруживаемой ошибкой)
                                           key(i) = key(i) Xor 255 'инвертируем ключ
                                       Next
                                       Dim paddingErrorDetected = False
                                       Try
                                           'Должно быть без ошибки, несмотря на некорректный ключ (используем выровненные данные
                                           'и внутренняя обработка без выравнивания))
                                           Dim dec2 = If(hashIterations = 0,
                                                         RijndaelEncryptor.Decode(enc, key, 100, notUsePadding), '100 итераций хеша
                                                         RijndaelEncryptor.Decode(enc, key, hashIterations, notUsePadding))
                                       Catch
                                           paddingErrorDetected = True
                                       End Try
                                       If paddingErrorDetected Then
                                           Throw New Exception("Padding error detected, but should be not")
                                       End If
                                   Next
                               Next
                           End Sub)
    End Sub
End Class

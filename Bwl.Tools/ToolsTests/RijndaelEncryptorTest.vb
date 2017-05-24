Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RijndaelEncryptorTest

    <TestMethod>
    Public Sub RijndaelEncryptorTest()
        Dim rnd As New Random(Now.Ticks Mod Integer.MaxValue)

        'С выравниванием
        For hashIterations = 1 To 4
            For dataByteCount = 1 To 100
                Dim data = New Byte(dataByteCount - 1) {} : rnd.NextBytes(data)
                For keyByteCount = 1 To 100
                    Dim key = New Byte(keyByteCount - 1) {} : rnd.NextBytes(key)
                    Dim usePadding = True 'Шифрование с выравниванием (данные любой длины), добавляется 1 байт к потоку кроме самого выравнивания
                    Dim enc = RijndaelEncryptor.Encode(data, key, hashIterations, usePadding)

                    'Нормальное дешифрование...
                    Dim dec = RijndaelEncryptor.Decode(enc, key, hashIterations, usePadding)
                    For check = 0 To data.Length - 1
                        If data(check) <> dec(check) Then
                            Throw New Exception("data(check) <> dec(check)")
                        End If
                    Next

                    '...и с новым ключом (должно быть с ошибкой)
                    rnd.NextBytes(key)
                    Dim paddingErrorCheck = False
                    Try
                        RijndaelEncryptor.Decode(enc, key, hashIterations, usePadding)
                    Catch
                        paddingErrorCheck = True
                    End Try
                Next
            Next
        Next

        'Без выравнивания
        For hashIterations = 1 To 4
            For dataByteCount = 32 To 3200 Step 32
                Dim data = New Byte(dataByteCount - 1) {} : rnd.NextBytes(data)
                For keyByteCount = 1 To 100
                    Dim key = New Byte(keyByteCount - 1) {} : rnd.NextBytes(key)
                    Dim notUsePadding = False 'Шифрование без выравнивания (ошибки расшифровки НЕ обнаруживаются, но данные всегда кратны 32 байтам)
                    Dim enc = RijndaelEncryptor.Encode(data, key, hashIterations, notUsePadding)

                    'Нормальное дешифрование...
                    Dim dec = RijndaelEncryptor.Decode(enc, key, hashIterations, notUsePadding)
                    For check = 0 To data.Length - 1
                        If data(check) <> dec(check) Then
                            Throw New Exception("data(check) <> dec(check)")
                        End If
                    Next

                    '...и с новым ключом (должно быть с необнаруживаемой ошибкой)
                    rnd.NextBytes(key)
                    Dim paddingErrorCheck = False
                    Try
                        RijndaelEncryptor.Decode(enc, key, hashIterations, notUsePadding)
                        paddingErrorCheck = True
                    Catch
                        paddingErrorCheck = False
                    End Try
                    If Not paddingErrorCheck Then
                        Throw New Exception("paddingErrorCheck")
                    End If
                Next
            Next
        Next

    End Sub
End Class

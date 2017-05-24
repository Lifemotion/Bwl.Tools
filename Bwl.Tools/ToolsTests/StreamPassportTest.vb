Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class StreamPassportTest

    <TestMethod>
    Public Sub StreamPassportTest()
        Dim rnd As New Random(Now.Ticks Mod Integer.MaxValue)
        For dataByteCount = 1 To 9000
            'Поток-оригинал
            Dim data = New Byte(dataByteCount - 1) {} : rnd.NextBytes(data)
            Dim stream = New MemoryStream() 'Точно так же можно контролировать целостность файлов
            stream.Write(data, 0, data.Length) : stream.Flush()

            Dim streamPassport = StreamPassportManager.Create("Stream1", stream)
            Assert.IsTrue(streamPassport.IsValid)

            'Проверка на корректность сериализации
            Dim streamPassportStr = streamPassport.Serialize()
            Dim streamPassport_ = New StreamPassport()
            streamPassport_.Deserialize(streamPassportStr)
            Assert.IsTrue(streamPassport_.IsValid)
            Assert.IsTrue(streamPassport_.Compare(streamPassport))
            Assert.IsTrue(streamPassport.Compare(streamPassport_))

            'Поток-оригинал, дополненный 1 байтом
            stream.WriteByte(255) : stream.Flush()
            Dim streamPassport2 = StreamPassportManager.Create("Stream2", stream)
            Assert.IsTrue(streamPassport2.IsValid())
            Assert.IsFalse(streamPassport.Compare(streamPassport2))
            Assert.IsFalse(streamPassport2.Compare(streamPassport))

            'Поток-оригинал, с 1 измененным байтом
            Dim data2 = data.Clone()
            Dim val = data2(0) : val += 1
            val = If(val > 255, 0, val)
            data2(0) = val

            Dim streamPassport3 = StreamPassportManager.Create("Stream3", stream)
            Assert.IsTrue(streamPassport3.IsValid())
            Assert.IsFalse(streamPassport.Compare(streamPassport3))
            Assert.IsFalse(streamPassport3.Compare(streamPassport))
        Next
    End Sub
End Class

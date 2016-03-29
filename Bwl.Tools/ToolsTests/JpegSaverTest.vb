Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class JpegSaverTest
    Dim _originalBmp As Bitmap

    <TestInitialize()>
    Public Sub Initialize()
        _originalBmp = CreateBitmapAtRuntime()
    End Sub
    <TestMethod()>
    Public Sub TestJpegSaverV1()
        Try
            
            Dim saver As New JpegSaver
            ' Вариант 1
            saver.Save("test.jpg", _originalBmp)
            Dim version1 = Bitmap.FromFile("test.jpg")

            ' Подгон оригинала под jpg
            Dim bmp = ToJpegFormat(_originalBmp)

            ' Проверки

            Assert.IsTrue(CheckByPixel(bmp,version1))

        Catch ex As Exception
            Throw
        End Try
    End Sub
        <TestMethod()>
    Public Sub TestJpegSaverV2()
        Try
            
            Dim saver As New JpegSaver

            ' Вариант 2
            Dim stream As New MemoryStream
            saver.Save (stream, _originalBmp)
            Dim version2 = Bitmap.FromStream(stream)
            stream.Dispose()


            ' Подгон оригинала под jpg
            Dim bmp = ToJpegFormat(_originalBmp)

            ' Проверки
            Assert.IsTrue(CheckByPixel(bmp,version2))

        Catch ex As Exception
            Throw
        End Try
    End Sub
        <TestMethod()>
    Public Sub TestJpegSaverV3()
        Try
            
            Dim saver As New JpegSaver

            ' Вариант 3
            Dim bmpArray = saver.SaveToBytes(_originalBmp)
            Dim stream = New MemoryStream(bmpArray)
            Dim version3 = Bitmap.FromStream(stream)
            stream.Dispose()

            ' Подгон оригинала под jpg
            Dim bmp = ToJpegFormat(_originalBmp)

            ' Проверки
            Assert.IsTrue(CheckByPixel(bmp,version3))

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Public Function CreateBitmapAtRuntime() As Bitmap

        Dim flag As New Bitmap(200, 100)
        Dim flagGraphics As Graphics = Graphics.FromImage(flag)
        Dim red As Integer = 0
        Dim white As Integer = 11
        While white <= 100
            flagGraphics.FillRectangle(Brushes.Red, 0, red, 200, 10)
            flagGraphics.FillRectangle(Brushes.White, 0, white, 200, 10)
            red += 20
            white += 20
        End While
        return flag

    End Function 

        Public Function ToJpegFormat(imageBmp As Bitmap) As Bitmap
        Dim _encoderParameters As New EncoderParameters(1)
        _encoderParameters.Param(0) = New EncoderParameter(Imaging.Encoder.Quality,90)
            Dim stream As New MemoryStream()
	            imageBmp.Save(stream, GetCodecInfo(ImageFormat.Jpeg), _encoderParameters)
        Dim bitmapImg = Drawing.Image.FromStream(stream)
        stream.Dispose()
        return bitmapImg
    End Function

        Private Function GetCodecInfo(ByVal format As ImageFormat) As ImageCodecInfo
        Dim codecs As ImageCodecInfo() = ImageCodecInfo.GetImageDecoders()
        Dim codec As ImageCodecInfo
        For Each codec In codecs
            If codec.FormatID = format.Guid Then
                Return codec
            End If
        Next codec
        Return Nothing
    End Function

        Public Function CheckByPixel(image1 As Bitmap, image2 As Bitmap) As Boolean

        If Not (image1.Height = image2.Height) Then Return False
        If Not (image1.Width = image2.Width) Then Return False

        For x = 0 To image1.Width - 1

            For y = 0 To image1.Height - 1
                If  Not (image1.GetPixel(x,y) = image2.GetPixel(x,y)) Then Return False
            Next

        Next
        Return True

    End Function
    
End Class
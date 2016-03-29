Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net.Mime
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ImageTest
    Dim image As Image
    Dim bitmap As Bitmap
    Dim originalFile As String

    Protected _rowData As Bitmap

    ''' <summary>Сырые данные изобаржения</summary>
    Public Property RowDataBytes As Byte()

    <TestInitialize()>
    Public Sub Initialize()
        image = New Image()
        bitmap = CreateBitmapAtRuntime()
        originalFile = "test.bmp"
        RowDataBytes = New Byte(){}
    End Sub

    <TestMethod()>
    Public Sub ImageTest()
        Try

            ' Освобождение места для файла
            If File.Exists(originalFile) 
                File.Delete(originalFile)
            End If

            ' Установка изображения
            image.SetImage(bitmap.Clone())
            ' Получение копии изображения
            Dim bitmapCopy = image.GetBitmap()

            ' Сохранение изображения
            image.SaveImage(originalFile)
            Dim bitmap2 As Bitmap = LoadImage(originalFile)

            Assert.IsTrue(CheckByPixel(bitmapCopy, bitmap))
            Assert.IsTrue(CheckByPixel(bitmap2, ToJpegFormat(bitmap)))
        
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Public Function CreateBitmapAtRuntime() As Bitmap

    Dim flag As New Bitmap(200, 200)
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

    Public Overridable Function LoadImage(ByVal fName As String) As Drawing.Image
        return Drawing.Image.FromFile(fName)
    End Function

    Public Function ToJpegFormat(imageBmp As Bitmap) As Bitmap
            Dim stream As New MemoryStream()
	            imageBmp.Save(stream, ImageFormat.Jpeg)
        Dim bitmapImg = Drawing.Image.FromStream(stream)
        stream.Dispose()
        return bitmapImg
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
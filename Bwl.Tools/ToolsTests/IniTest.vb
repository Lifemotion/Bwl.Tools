Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class IniTest

    <TestMethod()>
    Public sub TestIni()
        Dim ini As New IniFile("settings.ini")

        If Not (ini.IsFileExist())
            File.Create("settings.ini").Close()
        End If

        Dim original_value = "EverythingIsFine"
        ini.SetSetting("Default", "TestParameter", original_value)
        Dim value = ini.GetSetting("Default", "TestParameter")
        Assert.AreEqual(original_value, value)
    End sub

End Class
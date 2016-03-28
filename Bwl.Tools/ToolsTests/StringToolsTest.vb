Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class StringToolsTest

    <TestMethod()> Public Sub StringTools_CombineStrings()
        Dim result1 = StringTools.CombineStrings({"cat", "dog", "1"}, False, "#")
        Assert.AreEqual("cat#dog#1", result1)

        Dim result2 = StringTools.CombineStrings({"cat", "dog", "1"}, True, "#")
        Assert.AreEqual("1#dog#cat", result2)

        Dim result3 = StringTools.CombineStrings({"cat"}, False, "#")
        Assert.AreEqual("cat", result3)

        Dim result4 = StringTools.CombineStrings({"cat"}, True, "#")
        Assert.AreEqual("cat", result4)

        Dim result5 = StringTools.CombineStrings({}, True, "#")
        Assert.AreEqual("", result5)
    End Sub

End Class
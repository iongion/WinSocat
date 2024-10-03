using Firejox.App.WinSocat;

namespace APPTest;

public class StdPiperInfoTest
{

    [TestCase("STDIO")]
    public void VaildInputCheckTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(Firejox.App.WinSocat.StdPiperInfo.Check(element), Is.True);
    }

    [TestCase("STDIO")]
    [TestCase("stdio")]
    public void CaseInsensitiveValidInputCheckTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(Firejox.App.WinSocat.StdPiperInfo.Check(element), Is.True);
    }

    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void InvalidInputCheckTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(Firejox.App.WinSocat.StdPiperInfo.Check(element), Is.False);
    }
}
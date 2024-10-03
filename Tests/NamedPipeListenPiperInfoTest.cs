using Firejox.App.WinSocat;

namespace APPTest;

public class NamedPipeListenPiperInfoTest
{
    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe,ACL=AllowEveryone")]
    [TestCase("NPIPE-LISTEN:fooPipe,ACL=AllowCurrentUser")]
    public void VaildInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(NamedPipeListenPiperInfo.TryParse(element), Is.Not.Null);
    }

    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase("npipe-listen:fooPipe")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(NamedPipeListenPiperInfo.TryParse(element), Is.Not.Null);
    }

    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(NamedPipeListenPiperInfo.TryParse(element), Is.Null);
    }

    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe,ACL=AllowEveryone")]
    [TestCase("NPIPE-LISTEN:fooPipe,ACL=AllowCurrentUser")]
    public void PipePatternMatchTest(string input)
    {
        // Case 1 - Default ACL
        var element = AddressElement.TryParse("NPIPE-LISTEN:fooPipe");
        var parsed = NamedPipeListenPiperInfo.TryParse(element);
        Assert.That(parsed.PipeName, Is.EqualTo("fooPipe"));
        Assert.That(parsed.ACL, Is.EqualTo("AllowEveryone"));

        // Case 2 - AllowEveryone ACL
        element = AddressElement.TryParse("NPIPE-LISTEN:fooPipe,ACL=AllowEveryone");
        parsed = NamedPipeListenPiperInfo.TryParse(element);
        Assert.That(parsed.PipeName, Is.EqualTo("fooPipe"));
        Assert.That(parsed.ACL, Is.EqualTo("AllowEveryone"));

        // Case 3 - AllowCurrentUser ACL
        element = AddressElement.TryParse("NPIPE-LISTEN:fooPipe,ACL=AllowCurrentUser");
        parsed = NamedPipeListenPiperInfo.TryParse(element);
        Assert.That(parsed.PipeName, Is.EqualTo("fooPipe"));
        Assert.That(parsed.ACL, Is.EqualTo("AllowCurrentUser"));

    }
}

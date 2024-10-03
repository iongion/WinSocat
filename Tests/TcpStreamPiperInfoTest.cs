﻿using Firejox.App.WinSocat;

namespace APPTest;

public class TcpStreamPiperInfoTest
{
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP::80")]
    [TestCase("TCP:80")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(TcpStreamPiperInfo.TryParse(element), Is.Not.Null);
    }

    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("Tcp:127.0.0.1:80")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(TcpStreamPiperInfo.TryParse(element), Is.Not.Null);
    }

    [TestCase("STDIO")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(TcpStreamPiperInfo.TryParse(element), Is.Null);
    }

    [TestCase("TCP:127.0.0.1:80", ExpectedResult = "127.0.0.1")]
    [TestCase("TCP::80", ExpectedResult = "0.0.0.0")]
    [TestCase("TCP:80", ExpectedResult = "0.0.0.0")]
    public string HostPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return TcpStreamPiperInfo.TryParse(element).Host;
    }

    [TestCase("TCP:127.0.0.1:80", ExpectedResult = 80)]
    [TestCase("TCP::80", ExpectedResult = 80)]
    [TestCase("TCP:80", ExpectedResult = 80)]
    public int PortPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return TcpStreamPiperInfo.TryParse(element).Port;
    }
}
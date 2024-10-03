using System.IO.Pipes;
using System.Security.Principal;

namespace Firejox.App.WinSocat;

public class NamedPipeStreamPiperInfo
{
    private readonly string _serverName;
    private readonly string _pipeName;
    private readonly string _acl;

    public string ServerName => _serverName;
    public string PipeName => _pipeName;
    public string ACL => _acl;

    public NamedPipeStreamPiperInfo(string serverName, string pipeName, string acl)
    {
        _serverName = serverName;
        _pipeName = pipeName;
        _acl = acl;
    }
    public static NamedPipeStreamPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("NPIPE", StringComparison.OrdinalIgnoreCase)) return null!;
        
        string serverName;
        string pipeName;

        int sepIndex = element.Address.LastIndexOf(':');

        if (sepIndex == 0 || sepIndex == -1)
            serverName = ".";
        else
            serverName = element.Address.Substring(0, sepIndex);

        pipeName = element.Address.Substring(sepIndex + 1);

        return new NamedPipeStreamPiperInfo(serverName, pipeName, element.Options.GetValueOrDefault("ACL", "AllowEveryone"));
    }
}

public class NamedPipeListenPiperInfo
{
    private readonly string _pipeName;
    private readonly string _acl = "AllowEveryone";

    public string PipeName => _pipeName;
    public string ACL => _acl;

    public NamedPipeListenPiperInfo(string pipeName, string acl = "AllowEveryone")
    {
        _pipeName = pipeName;
        _acl = acl;
    }

    public static NamedPipeListenPiperInfo TryParse(AddressElement element)
    {
        if (element.Tag.Equals("NPIPE-LISTEN", StringComparison.OrdinalIgnoreCase))
            return new NamedPipeListenPiperInfo(element.Address, element.Options.GetValueOrDefault("ACL", "AllowEveryone"));

        return null!;
    }
}

public class NamedPipeStreamPiperFactory : IPiperFactory
{
    private readonly NamedPipeStreamPiperInfo _info;
    public NamedPipeStreamPiperInfo Info => _info;

    public NamedPipeStreamPiperFactory(NamedPipeStreamPiperInfo info)
    {
        _info = info;
    }

    public IPiper NewPiper()
    {
        var clientStream = new NamedPipeClientStream(
            _info.ServerName,
            _info.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous
        );
        
        clientStream.Connect();
        return new StreamPiper(clientStream);
    }

    public static NamedPipeStreamPiperFactory TryParse(AddressElement element)
    {
        NamedPipeStreamPiperInfo info;

        if ((info = NamedPipeStreamPiperInfo.TryParse(element)) != null)
            return new NamedPipeStreamPiperFactory(info);

        return null!;
    }
}

public class NamedPipeStreamPiperStrategy : PiperStrategy
{
    private readonly NamedPipeStreamPiperInfo _info;
    public NamedPipeStreamPiperInfo Info => _info;

    public NamedPipeStreamPiperStrategy(NamedPipeStreamPiperInfo info)
    {
        _info = info;
    }

    protected override IPiper NewPiper()
    {
        var clientStream = new NamedPipeClientStream(
            _info.ServerName,
            _info.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous
        );
        
        clientStream.Connect();
        return new StreamPiper(clientStream);
    }

    public static NamedPipeStreamPiperStrategy TryParse(AddressElement element)
    {
        NamedPipeStreamPiperInfo info;

        if ((info = NamedPipeStreamPiperInfo.TryParse(element)) != null)
            return new NamedPipeStreamPiperStrategy(info);

        return null!;
    }
}

public class NamedPipeListenPiper : IListenPiper
{
    private NamedPipeListenPiperInfo _info;
    private NamedPipeServerStream? _serverStream;
    private bool _closed;

    public NamedPipeListenPiper(NamedPipeListenPiperInfo info)
    {
        _info = info;
        _serverStream = null;
        _closed = false;
    }

    public void SetPermissions(NamedPipeServerStream _serverStream)
    {
        if (OperatingSystem.IsWindows())
        {
            // Only allow current user
            if (_info.ACL.Equals("AllowCurrentUser", StringComparison.OrdinalIgnoreCase))
            {
                var securityIdentifier = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
                var pipeAcl = new PipeAccessRule(securityIdentifier,
                    PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                    System.Security.AccessControl.AccessControlType.Allow);
                var pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(pipeAcl);
                _serverStream.SetAccessControl(pipeSecurity);
            }
        }
    }

    public IPiper NewIncomingPiper()
    {
        _serverStream = new NamedPipeServerStream(
            _info.PipeName,
            PipeDirection.InOut,
            -1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        SetPermissions(_serverStream);
        _serverStream.WaitForConnection();
        var tmpServerStream = _serverStream;
        _serverStream = null;
        return new StreamPiper(tmpServerStream);
    }

    public async Task<IPiper> NewIncomingPiperAsync()
    {
        if (_closed)
            throw new ObjectDisposedException("NamedPipeListenPiper is closed");
        
        _serverStream = new NamedPipeServerStream(
            _info.PipeName,
            PipeDirection.InOut,
            -1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        SetPermissions(_serverStream);
        await _serverStream.WaitForConnectionAsync();
        
        var tmpServerStream = _serverStream;
        _serverStream = null;
        return new StreamPiper(tmpServerStream);
    }

    public void Close()
    {
        _closed = true;

        if (_serverStream != null)
            _serverStream.Close();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        try
        {
            if (disposing && _serverStream != null)
            {
                _serverStream.Dispose();
            }
        }
        finally
        {
            _serverStream = null;
        }
    }
}

public class NamedPipeListenPiperStrategy : ListenPiperStrategy
{
    private readonly NamedPipeListenPiperInfo _info;
    public NamedPipeListenPiperInfo Info => _info;

    public NamedPipeListenPiperStrategy(NamedPipeListenPiperInfo info)
    {
        _info = info;
    }

    protected override IListenPiper NewListenPiper()
    {
        return new NamedPipeListenPiper(_info);
    }

    public static NamedPipeListenPiperStrategy TryParse(AddressElement element)
    {
        NamedPipeListenPiperInfo info;

        if ((info = NamedPipeListenPiperInfo.TryParse(element)) != null)
            return new NamedPipeListenPiperStrategy(info);

        return null!;
    }
}
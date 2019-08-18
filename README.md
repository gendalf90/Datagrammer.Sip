# Datagrammer.Sip

If you want to know more about Sip protocol please read this [wiki](https://en.wikipedia.org/wiki/Session_Initiation_Protocol).

### Getting started

Install from [NuGet](https://www.nuget.org/packages/Datagrammer.Sip/):

```powershell
Install-Package Datagrammer.Sip
```

Use namespace

```csharp
using Datagrammer.Sip;
using Sip.Protocol;
```

### Initialization

Building message bytes:

```csharp
var sipMessageBytes = new SipBuilderStep().SetRequestHeader("REGISTER", "sip:ss2.wcom.com")
                                          .AddHeader("Via", "SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh")
                                          .AddHeader("From", "SomeGuy <sip:User@there.com>")
                                          .AddHeader("Content-Length", "3")
                                          .SetBody(new byte[] { 1, 2, 3 })
                                          .Build();
```

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

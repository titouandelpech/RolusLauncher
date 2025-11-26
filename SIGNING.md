# Code Signing Guide

Code signing your launcher executable is highly recommended to:
- Prevent Windows SmartScreen warnings
- Build user trust
- Ensure the executable hasn't been tampered with

## Option 1: Self-Signed Certificate (For Testing)

1. Generate a self-signed certificate:
```powershell
New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=Rolus Launcher" -CertStoreLocation Cert:\CurrentUser\My
```

2. Export the certificate:
```powershell
$cert = Get-ChildItem -Path Cert:\CurrentUser\My -CodeSigningCert
Export-PfxCertificate -Cert $cert -FilePath "rolus-cert.pfx" -Password (ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText)
```

3. Sign the executable:
```bash
signtool sign /f "rolus-cert.pfx" /p "YourPassword" /t http://timestamp.digicert.com RolusLauncher.exe
```

**Note**: Self-signed certificates will still trigger warnings. For production, use a trusted certificate authority.

## Option 2: Trusted Certificate Authority (Recommended for Production)

### Purchase a Code Signing Certificate

1. Purchase from a trusted CA:
   - [DigiCert](https://www.digicert.com/code-signing/)
   - [Sectigo](https://sectigo.com/ssl-certificates-tls/code-signing)
   - [GlobalSign](https://www.globalsign.com/en/code-signing-certificate)

2. After receiving your certificate, export it as a PFX file

3. Sign the executable:
```bash
signtool sign /f "your-certificate.pfx" /p "YourPassword" /t http://timestamp.digicert.com RolusLauncher.exe
```

### Using signtool

`signtool.exe` is included with the Windows SDK. Common locations:
- `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\signtool.exe`
- `C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1\Bin\signtool.exe`

Add it to your PATH or use the full path.

## Verification

After signing, verify the signature:
```bash
signtool verify /pa RolusLauncher.exe
```

## Automated Signing

You can integrate signing into your build process by modifying `build.bat`:

```batch
@echo off
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo Signing executable...
    signtool sign /f "path\to\certificate.pfx" /p "password" /t http://timestamp.digicert.com bin\Release\net8.0-windows\win-x64\publish\RolusLauncher.exe
)
```

## Timestamping

Always use timestamping (`/t` parameter) so your signature remains valid even after the certificate expires.


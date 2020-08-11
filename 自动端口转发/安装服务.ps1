if ([System.Environment]::Is64BitOperatingSystem) {
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe .\自动端口转发.exe
} else {
  C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe .\自动端口转发.exe
}
Start-Service portfwd
pause

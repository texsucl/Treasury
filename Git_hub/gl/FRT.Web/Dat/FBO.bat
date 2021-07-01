@ECHO OFF
CD /d "%~dp0"
START fbofilecipher.exe %1 %2 %2.enc
EXIT
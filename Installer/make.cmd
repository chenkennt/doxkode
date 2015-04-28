
SET ARGC=1
FOR %%X IN (%*) DO (
    SET /A ARGC+=1
)

IF NOT "%ARGC%" == "3" (
    ECHO Usage: "%~f0" ^<installer output folder^ ^<root output folder^> 1>&2
    SET ERRORLEVEL=-1
    GOTO EXIT
)

SET INSTALLEROUTPUTFOLDER=%1
SET OUTPUTFOLDER=%2

wix\heat.exe dir "%OUTPUTFOLDER%BuildMeta" -gg -sfrag -templatefragment -dr INSTALLDIR -cg BuildMetaFiles -var var.BuildMetaDropFolder -out obj\buildmetafrag.wxs -sw5150 -sw5151
wix\heat.exe dir "%OUTPUTFOLDER%VsixSource" -gg -sfrag -templatefragment -dr DocascodeVsixDir -cg DocascodeVsixFiles -var var.VsixDropFolder -out obj\vsixfrag.wxs -sw5150 -sw5151

wix\candle.exe  -ext WixVsExtension installer.wxs obj\buildmetafrag.wxs obj\vsixfrag.wxs -dBuildMetaDropFolder="%OUTPUTFOLDER%BuildMeta" -dVsixDropFolder="%OUTPUTFOLDER%VsixSource" 
wix\light.exe -ext WixVsExtension -out "%INSTALLEROUTPUTFOLDER%docascode.msi" vsixfrag.wixobj buildmetafrag.wixobj installer.wixobj
ECHO %OUTPUTFOLDER%docascode.msi is successfully generated!

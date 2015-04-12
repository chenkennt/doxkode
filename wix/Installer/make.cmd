wix\heat.exe dir "..\..\Drop\debug\BuildMeta" -gg -sfrag -templatefragment -dr BUILDMETADIR -cg BuildMetaFiles -var var.BuildMetaDropFolder -out obj\buildmetafrag.wxs -sw5150 -sw5151
wix\heat.exe dir "..\..\Drop\debug\VsixSource" -gg -sfrag -templatefragment -dr DocascodeVsixDir -cg DocascodeVsixFiles -var var.VsixDropFolder -out obj\vsixfrag.wxs -sw5150 -sw5151

wix\candle.exe  -ext WixVsExtension installer.wxs obj\buildmetafrag.wxs obj\vsixfrag.wxs -dBuildMetaDropFolder="..\..\Drop\debug\BuildMeta" -dVsixDropFolder="..\..\Drop\debug\VsixSource" 
wix\light.exe -ext WixVsExtension -out docascode.msi vsixfrag.wixobj buildmetafrag.wixobj installer.wixobj

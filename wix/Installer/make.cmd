wix\heat.exe dir "..\..\Drop\debug\BuildMeta" -gg -sfrag -templatefragment -dr BUILDMETADIR -cg BuildMetaFiles -var var.BuildMetaDropFolder -out buildmetafrag.wxs -sw5150 -sw5151
wix\candle.exe  -ext WixVsExtension installer.wxs buildmetafrag.wxs -dBuildMetaDropFolder="..\..\Drop\debug\BuildMeta"
wix\light.exe -ext WixVsExtension -out docascode.msi buildmetafrag.wixobj installer.wixobj

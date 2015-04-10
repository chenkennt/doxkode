heat dir "..\Drop\debug" -gg -sfrag -template:fragment -out installer.wxs
candle installer.wxs
light installer.wixobj

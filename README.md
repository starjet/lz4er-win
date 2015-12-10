# lz4er-win
C# port of https://github.com/summertriangle-dev/more-ss-tools/blob/master/lz4er.c for Windows. Uses https://github.com/MiloszKrajewski/lz4net for lz4 decompression.

Usage: lz4er-win.exe path-to-lz4-file

New: Experimental repacking support. Requires original compressed file.

Usage: lz4er-win.exe -pack path-to-file-to-compress path-to-original-compressed-file

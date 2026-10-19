# Rebuild the multi-resolution Windows icon from the CrystalMix vector design.
Add-Type -AssemblyName System.Drawing
$root=Split-Path -Parent $PSScriptRoot
$assets=Join-Path $root 'CrystalMix\Assets'
function Points($values) {
    $list=[Collections.Generic.List[Drawing.PointF]]::new()
    for($i=0;$i -lt $values.Count;$i+=2){$list.Add([Drawing.PointF]::new($values[$i],$values[$i+1]))}
    return ,$list.ToArray()
}
function Polygon($g,$color,$values){$b=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($color)); $g.FillPolygon($b,(Points $values));$b.Dispose()}
$frames=@()
foreach($size in @(16,24,32,48,64,128,256,512)){
    $bmp=[Drawing.Bitmap]::new($size,$size)
    $g=[Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode='AntiAlias'; $g.ScaleTransform($size/256.0,$size/256.0)
    $path=[Drawing.Drawing2D.GraphicsPath]::new()
    foreach($arc in @(@(8,8,270),@(144,8,0),@(144,144,90),@(8,144,180))){$path.AddArc($arc[0],$arc[1],104,104,$arc[2]-90,90)}
    $path.CloseFigure()
    $bg=[Drawing.Drawing2D.LinearGradientBrush]::new([Drawing.Point]::new(8,8),[Drawing.Point]::new(248,248),[Drawing.ColorTranslator]::FromHtml('#17294c'),[Drawing.ColorTranslator]::FromHtml('#080f24'))
    $g.FillPath($bg,$path);$bg.Dispose();$path.Dispose()
    Polygon $g '#bcfff4' @(128,35,198,79,128,121,58,79)
    Polygon $g '#38cbd8' @(58,79,128,121,128,203,58,159)
    Polygon $g '#7b76ed' @(198,79,128,121,128,203,198,159)
    Polygon $g '#dcfff9' @(128,35,103,106,128,121,153,106)
    Polygon $g '#26a8c6' @(58,79,103,106,128,203,58,159)
    Polygon $g '#9193f4' @(198,79,153,106,128,203,198,159)
    $pen=[Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#d9ffff'),2.5)
    $g.DrawLines($pen,(Points @(58,79,128,121,198,79)));$g.DrawLine($pen,128,121,128,203);$pen.Dispose()
    $pen=[Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#7389ad'),3);$g.DrawLine($pen,85,222,171,222);$pen.Dispose()
    foreach($dot in @(@(85,'#66e8e3'),@(128,'#d0fcff'),@(171,'#a697ff'))){$b=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($dot[1]));$g.FillEllipse($b,($dot[0]-6),216,12,12);$b.Dispose()}
    $g.Dispose()
    $stream=[IO.MemoryStream]::new();$bmp.Save($stream,[Drawing.Imaging.ImageFormat]::Png)
    if($size -le 256){$frames+=,[pscustomobject]@{Size=$size;Bytes=$stream.ToArray()}}
    if($size -eq 512){$bmp.Save((Join-Path $assets 'CrystalMix.png'),[Drawing.Imaging.ImageFormat]::Png)}
    $stream.Dispose();$bmp.Dispose()
}
$out=[IO.File]::Create((Join-Path $assets 'CrystalMix.ico'));$writer=[IO.BinaryWriter]::new($out)
$writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$frames.Count)
$offset=6+16*$frames.Count
foreach($frame in $frames){
    $dimension=if($frame.Size -eq 256){0}else{$frame.Size}
    $writer.Write([byte]$dimension);$writer.Write([byte]$dimension);$writer.Write([byte]0);$writer.Write([byte]0)
    $writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$frame.Bytes.Length);$writer.Write([uint32]$offset)
    $offset+=$frame.Bytes.Length
}
foreach($frame in $frames){$writer.Write([byte[]]$frame.Bytes)}
$writer.Dispose(); 'Created icon sizes 16, 24, 32, 48, 64, 128 and 256.'

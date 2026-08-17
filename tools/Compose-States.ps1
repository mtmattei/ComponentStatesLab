# Crops each captured state to the target component and lays the four states out in a
# labelled 2x2 sheet, so the shell can be compared across states directly.
param(
    [Parameter(Mandatory = $true)][string]$Prefix,      # e.g. main / bare
    [Parameter(Mandatory = $true)][int]$ExpectedHeight, # component outer height
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [Parameter(Mandatory = $true)][string]$Heading
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$sp = Split-Path $OutputPath -Parent

function Find-Component($bmp, $expected) {
    # A border row is a long horizontal run of non-background pixels.
    $rowHits = @{}
    for ($y = 60; $y -lt $bmp.Height - 6; $y++) {
        $n = 0
        for ($x = 200; $x -lt 824; $x += 2) {
            $c = $bmp.GetPixel($x, $y)
            if ($c.R -lt 246 -or $c.G -lt 244) { $n++ }
        }
        if ($n -gt 200) { $rowHits[$y] = $n }
    }
    # Descending: a filled block higher up the page (the account header) can also produce a
    # matching row pair, so take the lowest match, which is the target component.
    $rows = $rowHits.Keys | Sort-Object -Descending
    foreach ($a in $rows) {
        $b = $a + $expected - 1
        if ($rowHits.ContainsKey($b)) {
            # left/right extent, measured a few px below the top border
            $probe = $a + 3
            $left = -1; $right = -1
            for ($x = 120; $x -lt 950; $x++) {
                $c = $bmp.GetPixel($x, $probe)
                if ($c.R -lt 246 -or $c.G -lt 244) { if ($left -lt 0) { $left = $x }; $right = $x }
            }
            return @{ Top = $a; Left = $left; Width = ($right - $left + 1) }
        }
    }
    return $null
}

$states = @(
    @{ Key = 'data'; Label = 'DATA' },
    @{ Key = 'loading'; Label = 'LOADING' },
    @{ Key = 'empty'; Label = 'EMPTY' },
    @{ Key = 'error'; Label = 'ERROR' }
)

$pad = 18
$crops = @()
foreach ($s in $states) {
    $path = Join-Path $sp "$Prefix-$($s.Key).png"
    $bmp = [System.Drawing.Bitmap]::FromFile($path)
    $hit = Find-Component $bmp $ExpectedHeight
    if ($null -eq $hit) { throw "component not located in $path" }
    $rx = [math]::Max(0, $hit.Left - $pad)
    $ry = [math]::Max(0, $hit.Top - $pad)
    $rw = [math]::Min($bmp.Width - $rx, $hit.Width + 2 * $pad)
    $rh = [math]::Min($bmp.Height - $ry, $ExpectedHeight + 2 * $pad)
    $rect = New-Object System.Drawing.Rectangle($rx, $ry, $rw, $rh)
    $crop = $bmp.Clone($rect, $bmp.PixelFormat)
    $crops += [pscustomobject]@{ Label = $s.Label; Img = $crop; W = $rect.Width; H = $rect.Height; Measured = "$($hit.Width) x $ExpectedHeight" }
    $bmp.Dispose()
    Write-Host ("{0,-8} located at y={1} left={2} size={3}" -f $s.Label, $hit.Top, $hit.Left, "$($hit.Width)x$ExpectedHeight")
}

$cw = [int](($crops | Measure-Object -Property W -Maximum).Maximum)
$ch = [int](($crops | Measure-Object -Property H -Maximum).Maximum)
$labelH = 30
$gap = 20
$margin = 28
$headH = 52

$sheetW = [int]($margin * 2 + $cw * 2 + $gap)
$sheetH = [int]($headH + $margin * 2 + ($ch + $labelH) * 2 + $gap)
Write-Host "sheet grid: cell ${cw}x${ch} -> sheet ${sheetW}x${sheetH}"

$sheet = New-Object System.Drawing.Bitmap -ArgumentList @([int]$sheetW, [int]$sheetH)
$g = [System.Drawing.Graphics]::FromImage($sheet)
$g.Clear([System.Drawing.Color]::FromArgb(255, 244, 243, 248))
$g.TextRenderingHint = 'ClearTypeGridFit'

$headFont = New-Object System.Drawing.Font('Segoe UI Semibold', 15)
$labFont = New-Object System.Drawing.Font('Consolas', 11, [System.Drawing.FontStyle]::Bold)
$dimFont = New-Object System.Drawing.Font('Consolas', 10)
$dark = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 28, 27, 34))
$grey = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 110, 106, 120))

$g.DrawString($Heading, $headFont, $dark, $margin, 16)

for ($i = 0; $i -lt 4; $i++) {
    $col = $i % 2
    $row = [math]::Floor($i / 2)
    $x = $margin + $col * ($cw + $gap)
    $y = $headH + $margin + $row * ($ch + $labelH + $gap)

    $g.DrawString($crops[$i].Label, $labFont, $dark, $x, $y)
    $sz = $g.MeasureString($crops[$i].Label, $labFont)
    $g.DrawString($crops[$i].Measured, $dimFont, $grey, ($x + $sz.Width + 6), ($y + 2))
    $g.DrawImage($crops[$i].Img, $x, ($y + $labelH))
}

$g.Dispose()
$sheet.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
$sheet.Dispose()
foreach ($c in $crops) { $c.Img.Dispose() }
Write-Host "sheet -> $OutputPath ($sheetW x $sheetH)"

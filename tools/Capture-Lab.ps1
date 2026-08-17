# Captures the ComponentStatesLab window with PrintWindow(PW_RENDERFULLCONTENT).
# Self-contained: PowerShell shell state does not persist between tool calls.
param(
    [Parameter(Mandatory = $true)][int]$ProcessId,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

if (-not ('LabCap.Win' -as [type])) {
    Add-Type -Namespace LabCap -Name Win -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool PrintWindow(System.IntPtr hwnd, System.IntPtr hdc, uint flags);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool GetWindowRect(System.IntPtr hWnd, out RECT r);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, System.IntPtr lParam);
public delegate bool EnumWindowsProc(System.IntPtr hWnd, System.IntPtr lParam);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern uint GetWindowThreadProcessId(System.IntPtr hWnd, out uint pid);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool IsWindowVisible(System.IntPtr hWnd);
public struct RECT { public int L, T, R, B; }
'@
}

# MainWindowHandle is unreliable for Uno Skia desktop windows - enumerate instead.
$script:target = [System.IntPtr]::Zero
$cb = [LabCap.Win+EnumWindowsProc] {
    param($h, $l)
    $o = 0
    [void][LabCap.Win]::GetWindowThreadProcessId($h, [ref]$o)
    if ($o -eq [uint32]$ProcessId -and [LabCap.Win]::IsWindowVisible($h)) {
        $r = New-Object LabCap.Win+RECT
        [void][LabCap.Win]::GetWindowRect($h, [ref]$r)
        if (($r.R - $r.L) -gt 200) { $script:target = $h }
    }
    return $true
}
[void][LabCap.Win]::EnumWindows($cb, [System.IntPtr]::Zero)

if ($script:target -eq [System.IntPtr]::Zero) { throw "No visible window found for PID $ProcessId" }

$r = New-Object LabCap.Win+RECT
[void][LabCap.Win]::GetWindowRect($script:target, [ref]$r)
$w = $r.R - $r.L
$h = $r.B - $r.T

$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
$ok = [LabCap.Win]::PrintWindow($script:target, $hdc, 2)
$g.ReleaseHdc($hdc)
$g.Dispose()
$bmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

# A TRUE return is not proof of pixels - sample for a near-uniform (blank) capture.
$cols = @{}
for ($y = 0; $y -lt $h; $y += 40) {
    for ($x = 0; $x -lt $w; $x += 40) { $cols[$bmp.GetPixel($x, $y).ToArgb()] = 1 }
}
$bmp.Dispose()

"hwnd=$($script:target) PrintWindow=$ok size=${w}x${h} distinctColors=$($cols.Count) -> $OutputPath"
if ($cols.Count -le 2) { Write-Warning "Capture looks blank - foreground the window and retry." }

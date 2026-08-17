# Launch the Release lab exe under a given LAB_PAGE / LAB_MODE, size the window so the
# whole page fits, capture it, then kill the process.
param(
    [Parameter(Mandatory = $true)][string]$Mode,
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [string]$Page = 'main',
    [int]$SettleMs = 4000,
    [int]$Width = 1024,
    [int]$Height = 820
)

$ErrorActionPreference = 'Stop'
$root = 'C:\Users\Platform006\ComponentStatesLab'
$exe = Join-Path $root 'ComponentStatesLab\bin\Release\net10.0-desktop\ComponentStatesLab.exe'

if (-not ('LabRun.Win' -as [type])) {
    Add-Type -Namespace LabRun -Name Win -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool SetWindowPos(System.IntPtr h, System.IntPtr after, int x, int y, int cx, int cy, uint flags);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, System.IntPtr lParam);
public delegate bool EnumWindowsProc(System.IntPtr hWnd, System.IntPtr lParam);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern uint GetWindowThreadProcessId(System.IntPtr hWnd, out uint pid);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool IsWindowVisible(System.IntPtr hWnd);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool GetWindowRect(System.IntPtr hWnd, out RECT r);
public struct RECT { public int L, T, R, B; }
'@
}

Get-Process ComponentStatesLab -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 600

$env:LAB_PAGE = $Page
$env:LAB_MODE = $Mode
$p = Start-Process -FilePath $exe -PassThru
$script:wantPid = [uint32]$p.Id

# Wait for the window, then size it. MainWindowHandle is unreliable here, so enumerate.
$script:hw = [System.IntPtr]::Zero
$cb = [LabRun.Win+EnumWindowsProc] {
    param($h, $l)
    $o = 0
    [void][LabRun.Win]::GetWindowThreadProcessId($h, [ref]$o)
    if ($o -eq $script:wantPid -and [LabRun.Win]::IsWindowVisible($h)) {
        $r = New-Object LabRun.Win+RECT
        [void][LabRun.Win]::GetWindowRect($h, [ref]$r)
        if (($r.R - $r.L) -gt 200) { $script:hw = $h }
    }
    return $true
}

for ($i = 0; $i -lt 40; $i++) {
    Start-Sleep -Milliseconds 250
    $script:hw = [System.IntPtr]::Zero
    [void][LabRun.Win]::EnumWindows($cb, [System.IntPtr]::Zero)
    if ($script:hw -ne [System.IntPtr]::Zero) { break }
}
if ($script:hw -eq [System.IntPtr]::Zero) { throw "window never appeared for $Page/$Mode" }

# SWP_NOMOVE(2) | SWP_NOZORDER(4) | SWP_NOACTIVATE(0x10)
[void][LabRun.Win]::SetWindowPos($script:hw, [System.IntPtr]::Zero, 0, 0, $Width, $Height, 0x16)

Start-Sleep -Milliseconds $SettleMs

& (Join-Path $root 'tools\Capture-Lab.ps1') -ProcessId $p.Id -OutputPath $OutputPath

Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300
Write-Host "captured $Page/$Mode -> $OutputPath"

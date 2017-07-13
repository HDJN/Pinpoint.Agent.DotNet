.\gacutil.exe -i Logger.Core.dll

$base_path = Split-Path -Parent $MyInvocation.MyCommand.Definition

$val = "COR_ENABLE_PROFILING=1
COR_PROFILER={941EC77E-F7C0-42C8-84E1-15FEFA3CE96F}
COR_PROFILER_PATH=" + $base_path + "\ILRewriteProfiler.dll
PINPOINT_HOME=" + $base_path + "\
PINPOINT_AGENT_ID=pp-20170713
PINPOINT_USER_FUNCTION_NAMESPACE=Logic"

Set-ItemProperty -type MultiString HKLM:\SYSTEM\CurrentControlSet\Services\WAS -name "Environment" -value $val
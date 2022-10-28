@ECHO OFF
SET configName=labo
copy chaperone_info.vrchap.%configName% chaperone_info.vrchap

IF ERRORLEVEL 1 (
REM Beep for attention
echo 
ECHO Enabling chaperone configuration "%configName%" failed
) ELSE (

ECHO Enabling chaperone configuration "%configName%" succeeded
)
timeout 3
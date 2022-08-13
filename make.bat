@ECHO OFF

if exist "build\" rmdir /s /q "build\"
if exist "bin\" rmdir /s /q "bin\"
if exist "autotrade" rmdir /s /q "autotrade
if exist "autotrade.zip" del "autotrade.zip"

cmake -G "Visual Studio 17 2022" -A x64 -S . -B build
cmake --build build --target ALL_BUILD --config Release --parallel

zip -r -q autotrade.zip autotrade/
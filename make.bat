@ECHO OFF

if exist "build\" rmdir /s /q "build\"
if exist "bin\" rmdir /s /q "bin\"

cmake -G "Visual Studio 17 2022" -A x64 -S . -B build
cmake --build build --parallel --config Release
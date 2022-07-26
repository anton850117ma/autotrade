cmake_minimum_required(VERSION 3.20)

project(autotrade LANGUAGES CSharp)

# Include CMake utilities for CSharp, for WinForm and WPF application support.
# include(CSharpUtilities)

# Define the executable, including any .cs files. 
# The .resx and other Properties files are optional here, but including them makes them visible in the VS solution for easy editing. 
add_executable(${CMAKE_PROJECT_NAME}
    ${CMAKE_SOURCE_DIR}/src/Program.cs
    ${CMAKE_SOURCE_DIR}/src/Utility.cs
    ${CMAKE_SOURCE_DIR}/src/EventHandler.cs
    ${CMAKE_SOURCE_DIR}/src/DataHandler.cs
    ${CMAKE_SOURCE_DIR}/src/RayinAPI.cs
)

# Set the .NET Framework version for the executable.
# set_property(TARGET PROEJECT_NAME PROPERTY DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")
# Set the executable to be 32-bit.
# set_property(TARGET ${CMAKE_PROJECT_NAME} PROPERTY WIN32_EXECUTABLE TRUE)
# Set the C# language version (defaults to 3.0).
set(CMAKE_CSharp_FLAGS "/langversion:latest")

# Disable warnings
target_compile_options(${CMAKE_PROJECT_NAME} PRIVATE "/nowarn:8632")

# Set the source file properties for Windows Forms use.
# csharp_set_windows_forms_properties(
#     Form1.cs
#     Form1.Designer.cs
#     Form1.resx
#     Program.cs
#     Properties/AssemblyInfo.cs
#     Properties/Resources.Designer.cs
#     Properties/Resources.resx
#     Properties/Settings.Designer.cs
#     Properties/Settings.settings
# )

# # If necessary, link in other library dependencies that were built locally in this source tree.
# target_link_libraries(PROEJECT_NAME ${CMAKE_CURRENT_BINARY_DIR}/lib/RayinVTS.dll)

# # If necessary, link in other library/DLL references, such as 3rd party libraries.
set_property(TARGET ${CMAKE_PROJECT_NAME} PROPERTY 
    VS_DOTNET_REFERENCE_RayinVTS RayinVTS.dll)

# # Add in the .NET reference libraries.
set_property(TARGET ${CMAKE_PROJECT_NAME} PROPERTY VS_DOTNET_REFERENCES
    "System"
    "System.Web"
    "System.Net.Http"
    "Microsoft.CSharp"
)

set_property(TARGET ${CMAKE_PROJECT_NAME} PROPERTY VS_PACKAGE_REFERENCES 
    "Newtonsoft.Json_13.0.1"
)

add_custom_command(TARGET ${CMAKE_PROJECT_NAME} POST_BUILD
                   COMMAND ${CMAKE_COMMAND} -E copy_directory 
                           ${PROJECT_BINARY_DIR}/Release ${CMAKE_SOURCE_DIR}/autotrade
                   COMMAND ${CMAKE_COMMAND} -E copy_directory 
                           ${CMAKE_SOURCE_DIR}/data ${CMAKE_SOURCE_DIR}/autotrade
                   COMMAND ${CMAKE_COMMAND} -E copy 
                           ${CMAKE_SOURCE_DIR}/RayinVTS.dll ${CMAKE_SOURCE_DIR}/autotrade
                   COMMAND ${CMAKE_COMMAND} -E copy 
                           ${CMAKE_SOURCE_DIR}/README.md ${CMAKE_SOURCE_DIR}/autotrade
                   COMMAND ${CMAKE_COMMAND} -E copy 
                           ${CMAKE_SOURCE_DIR}/data/Settings.json ${CMAKE_BINARY_DIR}/Release
                   COMMENT "Pack files completed!"
)
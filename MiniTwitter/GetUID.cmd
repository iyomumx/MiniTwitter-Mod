"C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
MSBuild /t:updateuid /p:Platform=x86 MiniTwitter.csproj
MSBuild /t:checkuid /p:Platform=x86 MiniTwitter.csproj
cd ..
MSBuild MiniTwitter.sln

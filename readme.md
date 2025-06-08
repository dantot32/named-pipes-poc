# Named pipes poc

```sh
dotnet new winforms -lang vb -n winform -f netcoreapp3.1
dotnet new console -n console
dotnet new sln
dotnet sln .\named-pipes-poc.sln add .\winform\Winform.vbproj
dotnet sln .\named-pipes-poc.sln add .\console\console.csproj
```

After the two projects has been created, change the target framework to 4.7.2.

```xml
<!-- in winform proj file -->
<TargetFramework>net472</TargetFramework>
```
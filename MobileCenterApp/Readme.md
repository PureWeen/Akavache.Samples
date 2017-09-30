# Setup of project

### MobileCenterApp.Shared
```xml
<PackageReference Include="akavache" Version="6.0.0-alpha0038" />   
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="1.1.8" />
<PackageReference Include="SQLitePCLRaw.core" Version="1.1.8" />
<PackageReference Include="Microsoft.Azure.Mobile.Analytics" Version="0.15.2" />
<PackageReference Include="Microsoft.Azure.Mobile.Crashes" Version="0.15.2" />
<PackageReference Include="Microsoft.Azure.Mobile.Distribute" Version="0.15.2" />
<PackageReference Include="Microsoft.Azure.Mobile.Push" Version="0.15.2" />
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="1.1.8">
    <ExcludeAssets>All</ExcludeAssets>
    <PrivateAssets>All</PrivateAssets>
</PackageReference>
```

https://docs.microsoft.com/en-us/dotnet/core/tools/csproj

>**ExcludeAssets** attribute specifies what assets belonging to the package specified by <PackageReference> should not be consumed.

>**PrivateAssets** attribute specifies what assets belonging to the package specified by <PackageReference> should be consumed but that they should not flow to the next project.


### MobileCenterApp.iOS

Only add the akavache.core package. *Akavache.Sqlite3* is netstandard and doesn't have any platform specific dlls that need to be baited in. It's ok just living in the shared project

```xml
<PackageReference Include="akavache.core">
    <Version>6.0.0-alpha0038</Version>
</PackageReference>
<PackageReference Include="SQLitePCLRaw.bundle_green">
    <Version>1.1.8</Version>
</PackageReference>
```

### Xamarin Forms App Startup

```C#
 public App ()
{

    InitializeComponent();

    SQLitePCL.Batteries_V2.Init();
    SQLitePCL.raw.FreezeProvider();
    _LazyBlob = new Lazy<IBlobCache>(() => new SQLitePersistentBlobCache("afile.db"));
    MainPage = new MobileCenterApp.MainPage();
}
```
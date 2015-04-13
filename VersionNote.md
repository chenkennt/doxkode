#Version Note
##v0.3-125-g704ed13 lianwei@4/13/2015
1. Use *Csharp Website* as *Documentation Website*'s template, move the *DocProjectVsix* and related *ConfigPublish* and *PublishDoc* projects into *Backup* folder
2. Add *Install.csproj* in folder *Install* to manage generating **docascode.msi**
3. Move Vsix related projects into folder *Vsix*

>**HOWTO** generate **docascode.msi**?
> Simply build *Install.csproj* and **docascode.msi** will be generated into *Drop\[debug|releasse]\Installer*
> **NOTE**
> Please install|uninstall extension throught **docascode.msi**
> The documentation project is now calling "DocumentationWebsite" and is hosted in CSharp project tab.
#Version Note
## lianwei@4/27/2015
1. Change BuildIndex to generate map file for each .yml and .md file instead of generating one md.yml file only.

## lianwei@4/20/2015
1. Change yaml file extension from .yaml to standard .yml
2. Change toc.yaml to use name-href pair
3. Flat yaml layout according to doc/metadata_format_spec.md


##v0.3-125-g704ed13 lianwei@4/13/2015
1. Use *Csharp Website* as *Documentation Website*'s template, move the *DocProjectVsix* and related *ConfigPublish* and *PublishDoc* projects into *Backup* folder
2. Add *Install.csproj* in folder *Install* to manage generating **docascode.msi**
3. Move Vsix related projects into folder *Vsix*

>**HOWTO** generate **docascode.msi**?
> Simply build *Install.csproj* and **docascode.msi** will be generated into *Drop\[debug|releasse]\Installer*
> **NOTE**
> Please install|uninstall extension throught **docascode.msi**
> The documentation project is now calling "DocumentationWebsite" and is hosted in CSharp project tab.
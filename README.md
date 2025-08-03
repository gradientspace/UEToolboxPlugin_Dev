# UEToolboxPlugin_Dev

This repository contains a development setup for the [Gradientspace UEToolbox plugin](https://github.com/gradientspace/UEToolboxPlugin). The repo for that plugin only contains the plugin code, which must be built inside a UE5 project. 
So, this repo contains such a project, configured with some test levels and assets that are useful for checking that (eg) building and packaging works properly.
Scripts for packaging the plugin for distribution on the FAB marketplace are also included.

The sample UE5.6 project is set up in **UEToolboxDev.uproject**. To use this project, (shift) right-click and **Generate Visual Studio Project Files...** from the context menu,
then open the generated **UEToolboxDev.sln** file. 

The UEToolbox plugin is included as a git submodule. If your git client doesn't automatically fetch submodules, run `git submodule update --init --recursive` after you check out this repo, or run *GIT_fetch_all_submodules.bat* in the root folder.



# Fab Build

The */FabPackaging* subdirectory contains scripts and utilities that I use to package this the plugin source for distribution on Epic's FAB marketplace ([see the listing here](https://www.fab.com/listings/0588e89f-d6fe-482d-882c-bd0e62d8e1d6)).
This FabPackager C# program included there does most of the steps, such as:

* automatically remove modules you might not want to publicly redistribute (eg internal/development modules)
* remove any empty files (will be rejected by fAB)
* optionally convert copyright strings to what FAB requires
* remove any PDB files, if you are packaging w/ embedded precompiled binaries
* create a zip file for each supported Engine Version

This build setup could easily be adapted to other plugins. Some things are hardcoded in FabPackager/Program.cs (easily modified).
The **CREATE_FAB_PLUGIN.bat** script copies from the plugin source from the UE Project's Plugins folder, and so would also need to be edited.
Output .zip files are generated in the SOURCE_DISTRIB folder.

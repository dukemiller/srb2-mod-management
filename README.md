# srb2-mod-management

A mod retriever and manager for the DOOM based Sonic fangame, [SRB2](https://www.srb2.org/). **[Find the download link for the latest version here.](https://github.com/dukemiller/srb2-mod-management/releases/latest)**  

### Description

In a game about speed, it's pretty slow to download and add mod files to the game for you to play. Navigating through the official forums (or wherever else), scrolling down to download, downloading the file, extracting it to the correct directory, either using some wad file manager or the in-game console to load the mod, maybe some other step if something went wrong, and then at that point you can begin playing. 

Not only that, but in that process your game folder (or wherever you load from) is polluted with a bunch of arbitrarily named files that may or may not be descriptive to what it contains. You try to  memorize what file correlates to what and if any mod has some other dependency, with it also being a lot of trouble to see if there are any updates to any mods you have. On top of that, small things like forgetting if you've completed something or not make it an annoying experience to come back to.

Instead, I've just made a hopefully simple interface to skip through some busywork to make the process maybe a little less annoying. Find more mods, go through the pages, click download, then either launch that mod alone or go to the home page and combine it with several other mods (with descriptive names and groupings) and hit play.

### Features

\- A few simple launch options with your settings saved transparently to your user profile (no need to juggle a .cfg file from folder to folder).   
\- An easier experience downloading and using mods from the official forums with minimal network overhead.  
\- Mods grouped by categories to be able to quickly distinguish what the mod does what.  
\- Access to the downloaded profile for any mod, complete with their description and screenshots (which are only downloaded when viewed). This will also notify you if the mod has an available update.  
\- A few other helpful things.

### Demo 

![Demo](https://i.imgur.com/ZVN1Phn.gif)

_(not all functionality shown)_

### Notes

I have very little emphasis on the netplay aspect of the game, and as such anything involving netplay or adding files to netplay is unsupported through this manager. You can obviously load files a server requires and then join that server, but it won't search in my provided  directories for available files.

It is **heavily suggested** that you use the mod manager itself to download and manage mods instead of locally adding every mod yourself. Using the download services within the program will store all the meta information about the mod (such as their profile page) and some core features will be unavailable to locally added mods (such as checking for updates or going to their profile). That being said, you are able to manually add your own if they are not available on the current mod source.

There are certain types of mods that I can't automatically implement, e.g. MD2 files, mods that use their own launcher and/or contain custom models, etc. To use those, manual use will be required (and is stated as so inside the program). I'm looking more into creating something like a MD2 profile swapper and such to solve some of these problems, so maybe at some point.

Support for Windows only. This is created using WPF, which you could say isn't well known for being friendly to unix like operating systems.

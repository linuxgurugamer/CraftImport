Greetings,

 

Updated for 1.4.1

updated for KSP 1.4.1
Deleted all unused KerbalX code
Added support for Toolbarcontrol and ClickthrougBlocker
Added option to load as assembly
Added code to check for comment after ship name  in craft file
Added localizer code in case ship name is localized
This mod now depends on and requires the ToolbarController mod, available here:


This mod now depends on and requires the ClickThroughBlocker mod, available here: 


I've just released a mod I've been working on called Craft Import. It is designed to allow you to import a craft file directly into the game by either downloading from an external website or a local file.

This will make it much easier to download files from KerbalX.com, among other places, all you will need is the download URL which you can simply paste into the entry field.

Download it here: http://spacedock.info/mod/47/Craft%20Import%20%26%20Upload

License is GPLv3

 

0.1.0 initial release
0.2.0 Fixed bug where showDrives wasn't being honored, so that drives were always being shown
0.3.0 Removed unnecessary dialog for file selection
   Added initial error checking for valid craft file, more needs to be done
   Changed file selection to only show directories and craft files
   Fixed bug where after you download one file, you can't do any more. Related to unnecessary dialog
0.4.0 Added integration with KerbalX. CKan file is downloaded and saved in case the imported craft file needs additional mods to be    used, and then provides instructions on screen
0.5.0 Changed visibility to only be visible in VAB and SPHIf no http/https/file is at the beginning of the craft name, checks to see if it is    a local file. If not, it prepends "http://"
0.7.0 Added upload ability to KerbalX
   Added "OK" button in case of error, will return to GUI screen with craftURL intact for retrying
   Upload functionality + ship snapshot
   Added option to load craft after import
   Added option to copy text to clipboard
   Made Import and Upload to KerbalX buttons bigger
   Changed spacing of elements on upload screen
   Added check to see if downloaded craft has all necessary mods
   Added check for login error
   Added check to see if all parts needed are available
   Generated image is dynamically updated as values are edited.
0.7.1 Removed code which prevented destruction of data on scene load
0.8.0 1.0.5 compatibility
0.8.1 Fixed issue with lagging during flight
   Fixed locking of screen when exiting editor while settings window was open
0.8.2 Fixed a couple of null ref exceptions
Blocked KerbalX from downloads due to security problem on site
Added message about kerbalx being blocked

Kudos to SpaceTiger, who very kindly show me how to load a subassembly in the editor

Donations gratefully accepted

Patreon.png

https://www.patreon.com/linuxgurugamer

Description

This is a small mod which will allow you to download and import craft files from websites. It will take a URL, which you can get from the site and just paste it into the URL field.

Additionally, if you have craft files on your local disk, you will be able to select and import them as well.

There is an additional field available to rename the craft to a new craft name if desired. This can be useful if, for example, you already have a craft by that name and don't want to overwrite it.

Also, if you DO want to overwrite an existing file, there is a toggle field which will allow you to do that.

Finally, if you like, you can save the craft file in the sandbox directory rather than the current games's directory

Available options include the ability to use the Blizzy Toolbar if it is available, and for the file selection dialog on Windows, to show all the available drive letters. If the drive letters are not shown, then you will be limited to the drive that the game is installed on

The buttons on the main dialog consist of the following:

[TABLE=width: 500]

[TR]

[TD]Select Local File[/TD]

[TD]Open the file selection dialog[/TD]

[/TR]

[TR]

[TD]Import[/TD]

[TD]Import the craft listed in the URL field[/TD]

[/TR]

[TR]

[TD]Cancel[/TD]

[TD][/TD]

[/TR]

[/TABLE]

The toolbar icon looks like this:

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/CI-toolbar.png

The main dialog:

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg1.png

The file selection shows the folders and files in the current directory. Only craft files and folders are selectable.

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg3.png

This dialog shows what it looks like after I descended into the sandbox ships directory, in the VAB:

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg4.png

and this is after I selected a craft file to import. Note that for local files, the complete file name is actually transformed into a URL by adding "file://:" in front of it.

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg5.png

This is where I'm ready to download a craft file from Kerbalx.com. Note that for Kerbalx, it seems to work so that once you get to the web page of the craft you want, that using the url is sufficient, no need to copy the "Download" url

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg6.png

and this is what happens when you try to overwrite a file when you didn't select the overwrite toggle:

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg7.png

This is a major update to the Craft Import mod.

There are two new additions:

1. When downloading a file from KerbalX.com, it will also download a CKAN file which can be used by CKAN to install any missing mods. When you do a download, instructions will be provided after the craft file is downloaded.

2. Craft Import now has the ability to UPLOAD to KerbalX.com.

You will need a login to KerbalX in order to do this, of course.

So, here is how it works:

The Craft Import button is only available in the VAB or SPH.

Very Important: Please note, any craft you have loaded in the editor will be lost and replaced with the craft being uploaded!

On the main Craft Import screen, click the button which says "Upload to KerbalX". You will be presented with the

File Selection dialog:

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg3.png

If you are in the VAB, the VAB directory of your current save will be displayed. If you are in the SPH, the SPH

directory will be shown.

Select your file, either by double-clicking or clicking one and then clicking the Select button

You will then get to the Craft Import screen:

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/kerbalx-1.png

There are two required fields, highlighted in yellow:

Userid This is your login id to KerbalX.com

Password This is your password to KerbalX.com

Picture URL This label is also a button. The field is a url to a previously uploaded picture of the craft.

If using Imgur, be sure to select the Direct Link url, as the others will generate an error. You can also

specify an Imgur album here. If specifying an Imgur album, you need to use one of three formats:

1. The "Share Link" from Imgur

2. the 5 character album identifier

3. The album identified surrounded by imgur tags as follows (note that there shouldn't be spaces between the brackets):

[ imgur ]12345[ /imgur ]

If you press the button, then a dialog will open up where you will be able to select a local image file (either PNG or JPG). It will initially open up in the screenshots directory of the current install.

If no picture is specified (either URL or local file), then a 1024x1024 image will be automatically generated and uploaded.

WARNING: Any images uploaded in this manner will be stored on Imgur, and will be undeletable. You will be able to replace it on the web page, but the image will stay on Imgur forever!

Generate a custom snapshot This button will bring up a dialog where you will be able to generate a snapshot of the craft with a custom background color.

There are check boxes to save both the userid and password. THESE ARE SAVED IN CLEAR TEXT! So only select them if you are comfortable with having them stored on your disk.

Other fields on the screen are:

Tags Space separated string of words used on KerbalX as ship tags

Video URL URL to a YouTube video

Action Groups This button brings up a new window where you can enter descriptions of the action groups which are set:

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/kerbalx-snapshot.png

The top line of this screen shows the name of the craft you are uploading.

the next set of lines control the camera:

Display Resolution This controls the size of the image window on screen.

It will never be larger than the Image Resolution. Changing this takes effect

immediately.

Image Resolution This is the final size of the picture (will be square)

The following values control the camera. The default values are set to what

KSP uses when generating a thumbnail in either the VAB or SPH, depending on

the craft. Change them slowly, one at a time to get a feel for how they work.

Elevation

Pitch

Heading

FOV Smaller values effectively moves the camera away from the ship

Next will be the background color. The label:

"Click on desired background color" will be the color of the background color

you select.

There are 4 buttons on the screen:

Generate Craft Image The image will not be generated (or regenerated) until

you click this button. When you click this for the first time, the main

window will be moved to the right edge, and the craft image will be displayed

in another window on the left side.

Reset to VAB Defaults (or SPH) This will reset the values

OK Accept this image for upload

Cancel Discard the changes

https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/kerbalx-actiongroups.png

If the craft has a B9 info drive, this will be ignored. Right now, KerbalX only stores the standard Action Groups. If in the future KerbalX is enhanced to support the Action Groiups Extended (250 action groups), this will be mirrored in Craft Import. This i

There is a toggle which say "Force new upload (if existing)". This means that if the craft you are uploading already exists, KerbalX will create a new craft and append a number to keep them unique.

If you upload a craft which already exists and you did not select the force new upload, you will be presented with a window showing the existing craft which match what you are uploading. The middle column will be a clickable button, which will open that craft in a browser window. Once you have decided which one to replace, select the toggle in the right column and then click OK.

Special note: If you know you are going to replace an existing craft, you can instead click the button "Select Existing Craft to Replace", a list of all your existing craft will be downloaded and displayed. If you do this, then you can replace ANY of your existing craft, not just the ones which match the craft you are uploading. Once you have filled out the information, click the button "Upload to KerbalX".

The progress will be displayed in a counter. When completed, the results will be displayed. If it was a successful upload, a browser window will be opened to allow you to do any additional editing of the craft record on KerbalX

Picture Urls are verified before uploading. YouTube Urls are validated for correct syntax before uploading.


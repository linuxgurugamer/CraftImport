This is a major update to the Craft Import mod.

There are two new additions:

1.  When downloading a file from KerbalX.com, it will also download a CKAN file which can be used by CKAN to install any missing mods.  When you do a download, instructions will be provided after the craft file is downloaded.

2.  Craft Import now has the ability to UPLOAD to KerbalX.com.

You will need a login to KerbalX in order to do this, of course.

So, here is how it works:

The Craft Import button is only available in the VAB or SPH.

Very Important:  Please note, any craft you have loaded in the editor will be lost and replaced with the craft being uploaded!

On the main Craft Import screen, click the button which says "Upload to KerbalX".  You will be presented with the File Selection dialog:

[https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/ci-pg3.png]

If you are in the VAB, the VAB directory of your current save will be displayed.  If you are in the SPH, the SPH directory will be shown.

Select your file, either by double-clicking or clicking one and then clicking the Select button


You will then get to the Craft Import screen:

[https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/kerbalx-1.png]

There are two required fields, highlighted in yellow:

Userid		This is your login id to KerbalX.com
Password	This is your password to KerbalX.com



Picture URL	This label is also a button.  The field is a url to a previously uploaded picture of the craft.  If using Imgur, be sure to select the Direct Link url, as the others will generate an error.  You can also specify an Imgur album here.  If specifying an Imgur album, you need to use one of three formats:
	1.  The "Share Link" from Imgur
	2.  the 5 character album identifier
	3.  The album identified surrounded by imgur tags as follows:
		[imgur]12345[/imgur]

If you press the button, then a dialog will open up where you will be able to
select a local image file (either PNG or JPG).  It will initially open up in
the screenshots directory of the current install.

If no picture is specified (either URL or local file), then a 1024x1024 image will be
automatically generated and uploaded.

WARNING:  Any images uploaded in this manner will be stored on Imgur, and will
be undeletable.  You will be able to replace it on the web page, but the image
will stay on Imgur forever!

Generate a custom snapshot	This button will bring up a dialog where you
will be able to generate a snapshot of the craft with a custom background
color.


There are check boxes to save both the userid and password.  THESE ARE SAVED IN CLEAR TEXT!  So only select them if  you are comfortable with having them stored on  your disk.


Other fields on the screen are:

Tags		Space separated string of words used on KerbalX as ship tags
Video URL	URL to a YouTube video
Action Groups	This button brings up a new window where you can enter descriptions of the action groups which are set:

[https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/kerbalx-snapshot.png]
The top line of this screen shows the name of the craft you are uploading.
the next set of lines control the camera:

Display Resolution	This controls the size of the image window on screen.
It will never be larger than the Image Resolution.  Changing this takes effect
immediately.
Image Resolution	This is the final size of the picture (will be square)
The following values control the camera.  The default values are set to what
KSP uses when generating a thumbnail in either the VAB or SPH, depending on
the craft.  Change them slowly, one at a time to get a feel for how they work.
Elevation		
Pitch
Heading
FOV		Smaller values effectively moves the camera away from the ship

Next will be the background color.  The label:
"Click on desired background color" will be the color of the background color
you select.

There are 4 buttons on the screen:

Generate Craft Image	The image will not be generated (or regenerated) until
you click this button. When you click this for the first time, the main
window will be moved to the right edge, and the craft image will be displayed
in another window on the left side. 
Reset to VAB Defaults (or SPH)  This will reset the values
OK			Accept this image for upload
Cancel			Discard the changes





[https://raw.githubusercontent.com/linuxgurugamer/SpaceTux/gh-pages/images/CraftImport/kerbalx-actiongroups.png]

If the craft has a B9 info drive, this will be ignored.  Right now, KerbalX only stores the standard Action Groups. If in the future KerbalX is enhanced to support the Action Groiups Extended (250 action groups), this will be mirrored in Craft Import.  This i

There is a toggle which say "Force new upload (if existing)".  This means that if the craft you are uploading already exists, KerbalX will create a new craft and append a number to keep them unique.

If you upload a craft which already exists and you did not select the force new upload, you will be presented with a window showing the existing craft which match what you are uploading.  The middle column will be a clickable button, which will open that craft in a browser window.  Once you have decided which one to replace, select the toggle in the right column and then click OK.

Special note: If you know you are going to replace an existing craft, you can instead click the button "Select Existing Craft to Replace", a list of all your existing craft will be downloaded and displayed.  If you do this, then you can replace ANY of your existing craft, not just the ones which match the craft you are uploading.

Once you have filled out the information, click the button "Upload to KerbalX".

The progress will be displayed in a counter.  When completed, the results will be displayed.  If it was a successful upload, a browser window will be opened to allow you to do any additional editing of the craft record on KerbalX

Picture Urls are verified before uploading.  YouTube Urls are validated for correct syntax before uploading.



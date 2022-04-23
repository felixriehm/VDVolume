/** \page pageInstallation Installation
 * 
 * To installVDVolume to your Unity project follow these instructions:
 *
 * <ul>
 * <li> Import the <b>VDVolume.unitypackage</b> into your project. You can find the VDVolume package in the folder <i>Export</i> of the source code.
 * <li> Install Unity's <b>TextMesh Pro</b> if you haven't already.
 * <li> Install the <b>Unity Entities package</b> via the package manager or else visit the <a href="https://docs.unity3d.com/Packages/com.unity.entities@0.11/manual/index.html">documentation</a>
 * for more information (for development <b>Version 0.11.2-preview.1 - September 17, 2020</b> was used). The package is used for Unity's Job System and Native Collections.
 * <li> Change the scripting <b>Api Compatibility Level</b> to <b>.Net 4.x</b> (<i>Edit->Project Settings->Player->Other Settings->Configuration</i>).
 * <li> Create a <b>csc.rsp</b> file inside your <i>Assets</i> forlder if you haven't already.
 * <li> Add <b>-r:System.Drawing.dll -unsafe</b> to <b>csc.rsp</b> (reference to System.Drawing and 'unsafe' code is used for loading images into a .NET Bitmap object).
 * <li> Optionally you can enable <b>Allow 'unsafe' Code</b> in the GUI to have the GUI in sync (<i>Edit->Project Settings->Player->Other Settings</i>).
 * </ul>
 *
 */
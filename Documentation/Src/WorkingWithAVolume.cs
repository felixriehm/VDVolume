/** \page pageWorkingWithAVolume Working With A Volume
 * 
 * When working with a volume there are five aspects to be aware of:
 * 
 * \section secCreation Creation Of A VDVolume
 * In order to have a %VDVolume in your scene you have either drag the <b>VDVolume</b> prefab from the prefab folder into the scene
 * or manually add the <b>Volume</b> script to a object.
 * 
 * \section secInstallFromAssetStore Initialization
 *
 * \subsection secInit Volume initialization data
 * A volume provides two different ways for receiving volume data. The first one is to <b>manually type</b> the <b>file path</b>, <b>scale</b> and
 * <b>chunk size</b> into the inspector fields of the volume object in your scene.
 * \image html vdvolume_inspector.png "VDVolume inspector" width=500px
 * The chunk size has to be a positive integer and the scale
 * has to be a positive float. When providing a file path you can choose to type in an absolute path or releative path. When choosing a relative path
 * you have to begin your path with <b>@relative:</b>.
 * \par
 * <b>Example:</b> @relative:myFolder/myVolume.vdvolume
 * 
 * The other option is more flexible allowing
 * you to <b>store these data inside a VolumeInitData object</b>. This allows you to swap between VolumeInitData objects without retyping you values.
 * You can <b>create a VolumeInitData object</b> by clicking on the menu item <b>VDVolume/Create VolumeInitData</b>. After that you can insert your values there and assign
 * the VolumeInitData object to the volume object in your scene.
 * \note This will overwrite your manually typed values.
 * 
 * \image html vdvolume_menu.png "VDVolume menu" width=300px
 * \image html vdvolume_initdata.png "VolumeInitData" width=500px
 * 
 * \subsection secMomentOfInit Moment of initialization
 * Furthermore you can define when the provided volume data will be initialized. This can be done by checking the <b>InitializeOnStart</b> checkbox
 * in the inspector of your volume object. If this is <b>true</b> it will initialize the data on Unity's <b>Awake() event function</b>.
 * If this is <b>false</b> you have to initialize the volume by the method called <b>InitVolume()</b> which is a member of the Volume class.
 * This method helps you to delay the initialization if needed.
 *
 * \note The loading process is asynchronous. You will see a billdboard with a progress bar in the scene when initializing a volume.
 * You can change the scaling and appearance of the billboard by modifying the <b>SerializationBillboard</b> prefab inside <i>VDVolume/Rresources/Prefabs</i>.
 * 
 * \image html vdvolume_progressbar.png "VDVolume progressbar" width=400px
 * 
 * \subsection secColliderType Collider type
 * The collider of the volume will be generated automatically. You can specify with <b>ColliderIsTrigger</b> in the inspector if the generated collider
 * should be set as <b>Trigger</b> or not. 
 * 
 * \section secSavingAVolume Saving A Volume
 * When saving a volume you have to call the method <b>AsyncSaveVolume()</b> of the <b>Volume</b> class. This is
 * a asynchronous process. Like the initialization a billboard with a progress bar will be shown in the scene for the duration of the process.
 *
 * \section secSetScale Set A Volume Scale
 * You can change the scaling of the volume after initialization by calling the method <b>SetScale()</b>.
 *
 * \section secEvents Volume Events
 * When developing you might want to listen to some events the Volume object is producing. There are the following events:
 * <ul>
 * <li> <b>OnVolumeInitialized</b>: This will trigger when the provided volume data has been fully initialized.
 * <li> <b>OnVolumeScaleChanged</b>: This will trigger when the scale of the volume has changed.
 * <li> <b>OnVolumeSavingStarted</b>: This will trigger when the the saving process has started.
 * <li> <b>OnVolumeSavingEnded</b>: This will trigger when the the saving process has ended.
 * </ul>
 *
 */
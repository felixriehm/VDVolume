/** \page pageGettingStarted Getting Started
 * 
 * To quickly display a volume and cut it follow these instructions:
 *
 * <ul>
 * <li> Place a <b>VDVolume</b> prefab from <i>VDVolume/Resources/Prefabs</i> into the scene.
 * <li> Select the object and assign the VolumeInitData <b>exampleVDVolume</b> from <i>VDVolume/Resources/VolumeInitData</i> to it or manually type your values.
 * The volume should now be displayed when starting the scene.
 * <li> For cutting place a <b>SphereCuttingCollider</b> prefab into the scene.
 * <li> Select the added object and assign the <b>VDVolume</b> of your scene to both scripts <b>SphereVolumeCollider</b> and <b>VolumeCollidable</b>
 * <li> You can now start the scene and move the sphere into the volume which will remove voxels in the process.
 * </ul>
 *
 */
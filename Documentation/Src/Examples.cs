/** \page pageExamples Examples
 * 
 * You can find different kind of examples in the source folder:
 *
 * \section secCodeExamples Code examples
 * There are three code examples in the <i>VDVolume/Scripts/Examples</i> folder which can be applied to a sphere
 * or cube which should remove voxels. The three examples are (see more information inside these files):
 * <ul>
 * <li> <b>CubeCuttingRayCast.cs</b>: Shows how you can cut a volume with <b>ray casting</b> for collision detection. This assumes you
 * have a cube as an object.
 * <li> <b>SphereCuttingRayCast.cs</b>: Shows how you can cut a volume with <b>ray casting</b> for collision detection. This assumes you
 * have a sphere as an object.
 * <li> <b>SphereVolumeColllider.cs</b>: Shows how you can cut a volume with <b>colliders</b> for collision detection. This assumes you
 * have a sphere as an object. It also shows you how to work with a <b>VolumeCollidable</b> when changing the object <b>scale</b>. Furthermore you can <b>undo</b> voxels with
 * this script and <b>restore</b> voxels.
 * </ul>
 * 
 * \section secPrefabs Availalbe prefabs
 * The prefabs can be found inside <i>VDVolume/Resources/Prefabs</i>:
 * <ul>
 * <li> <b>CubeCuttingRayCast</b>: A prefab for the equal named example script.
 * <li> <b>CollidableChisel</b>: A prefab for a custom collidable chisel.
 * <li> <b>SphereCuttingCollider</b>: A prefab for the equal named example script.
 * <li> <b>VDVolume</b>: A prefab for a volume.
 * </ul>
 *
 *  \section secExampleScenes Example scenes
 * The scenes can be found inside <i>VDVolume/Scenes</i>:
 * <ul>
 * <li> <b>UserExample</b>: Shows a simple scene with a VDVolume and two objects to cut, restore and undo the volume. It shows ray cast and collider methods.
 * <li> <b>CustomCollidableExample</b>: Shows a scene with a VDVolume and a custom collidable object (a chisel) to cut the volume.
 * </ul>
 *
 * \section secExampleVolumeData Volume data examples
 * The volume data examples can be found inside <i>VDVolume/Resources/VolumeInitData</i> and 
 * <i>StreamingAssets/VDVolume</i>:
 * <ul>
 * <li> <b>exampleVDVolume.asset</b>: An example <b>VolumeInitData</b> object for the example data inside StreamingAssets
 * <li> <b>ImportExample</b>: Contains <b>example images</b> which can be imported and an already generated <b>.vdvolume file</b>.
 * </ul>
 * 
 */
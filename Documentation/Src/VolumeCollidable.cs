/** \page pageVolumeCollidable Working with colliders
 * 
 * You can create a custom object which collides with the voxels of the volume. To do this you have to add the 
 * <b>VolumeCollidable</b> script to that object. When doing so you have to assign a <b>Volume</b> object to that script.
 * Your VolumeCollidable should have any type of Unity's Collider attached to it otherwise the script will add a default one. The collider
 * of your object can be any shape.
 * When working with a VolumeCollidable you can change its CollisionType. Furthermore you have to manage the scaling of
 * the VolumeCollidable script by yourself when you change the scaling of your object.
 *
 * \image html vdvolume_chisel.png "VDVolume chisel example" width=700px
 *
 * \section secCollisionType Collision Type
 * You can change the collision type of a collidable by setting the property <b>CollisionTyp</b>. There are following
 * collision types supported:
 * <ul>
 * <li> <b>Solid</b> (default): This will collide the collidable object with solid voxels. Suitable when cutting voxels.
 * <li> <b>Removed</b>: This will collide the collidable object with removed voxels. Suitable when restoring voxels.
 * <li> <b>None</b>: This will disable the collision.
 * </ul>
 * 
 * \section secHandlingCollisions Handling Collisions
 * When a collision occours you can retrieve the voxel data of the other collider by getting the <b>ColliderData</b>
 * component of the other object (otherCollider.gameObject.GetComponent<ColliderData>()).
 * ColliderData has the property <b>VoxelData</b> which you can access.
 *
 * \section secReqAndLimit Requirments, Limitations And Best Practices
 * \subsection secScaling Scaling
 * When the scale of the collisable object changes you have to call <b>ReInitCollidable()</b>. This will recalculate
 * the colliders the VolumeCollirable script generates. If you scale your object and call this method in the same frame you have to call Physics.SyncTransforms()
 * before calling this method. Otherwise the collider of your scaled object hasn't changed. Alternatively you
 * could wait for the next frame and pass the collider inside Unity's Update() function.
 * \note If you want to see the colliders in the scene hierarchy the VolumeCollidable script generates, you have to
 * call the method <b>SetHideInHierarchy()</b>. Per default the <b>colliders are hidden</b>.
 *
 * \subsection secColliderMesh Custom Mesh Collider
 * If you are working with <b>MeshCollider</b> you can assign the object mesh used for rendering to the MeshCollider component and set 
 * the property <b>Convex</b> to true. For better performance it is recommended to only span a collider around an
 * area where the object should be hittable (for example the head of a hammer). With this, the box collider grid which is 
 * generated around the your object collider through the script is smaller.
 *
 * \subsection secAmountOfColliders Volume Resolution VS VolumeCollidable Object Scaling
 * The VolumeCollidable script will create a grid of colliders around your object when voxels are in the close vicinity.
 * Depending on the resolution of the volume or the scaling of your collidable object the VolumeCollidable script will
 * create too many colliders which results in bad performance. To prevent this there is a limit to how many of these
 * colliders can be generated. You can change this limit inside the <b>VolumeCollidable</b> class. There you will find
 * the property <b>ColliderLimit</b>. The <b>default is set to 8.000</b>. However you can change this value however you like.
 * 
 */
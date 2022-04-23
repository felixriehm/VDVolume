/** \page pageVolumeModifiers Volume Modifiers
 * 
 * The only way to modify a volume is through the <b>VolumeModifiers</b> class which you can access through the <b>Volume</b>
 * object with the property <b>VolumeData</b>. There are four properties the class has which
 * allow you to modify a volume in following ways:
 *
 * \section secCutting Cutting
 * Cutting means removing voxels from the volume. You can do this with the following mehtods (see the class list for more
 * details):
 * <ul>
 * <li> <b>RemoveVoxel()</b>
 * <li> <b>RemoveSphere()</b>
 * <li> <b>RemoveCubeParallel()</b>
 * </ul>
 * 
 * \section secRestoring Restoring
 * Restoring means adding voxels back where they were previously. You can do this with the following methods (see the class list for more
 * details):
 * <ul>
 * <li> <b>AddVoxel()</b>
 * <li> <b>AddCubeParallel()</b>
 * </ul>
 *
 * \section secUndo Undo
 * The undo operation is currently only supported for cutted or restored voxels. You can't undo a unlimited amount of voxels.
 * You can specify the amount of the last number of voxels inside the <b>VoxelCmdBuffer</b> class. There you will find a variable <b>BufferSize</b>
 * which is per default set to <b>262.144</b>. You can change this value however you like. You can undo the last voxels
 * with the methods <b>UndoVoxels()</b>.
 * 
 * \section secFilter Filter
 * You can filter a value range or a single value. When filtering the voxels will be removed. The following methods
 * exist to filter a volume (see the class list for more details):
 *
 * <ul>
 * <li> <b>FilterValue()</b>
 * <li> <b>FilterValueRange()</b>
 * </ul>
 * \image html vdvolume_filtering.png "VDVolume filter" width=700px
 * 
 */
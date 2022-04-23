using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace VDVolume.Modifiers.Undo
{
    /// <summary>
    /// This class holds and manages the voxel buffer used for the undo process. The buffer is implemented as a ring buffer.
    /// </summary>
    internal class VoxelCmdBuffer
    {
        /// <summary>
        /// Defines the size of the ring buffer. Default is 262144 = 2^18.
        /// </summary>
        private const int BufferSize = 262144; // TODO: Test if size is 10
        /// <summary>
        /// The actual ring buffer.
        /// </summary>
        private NativeArray<VoxelCmdData> _removedVoxelBuffer;
        /// <summary>
        /// Index which always points to the next to be written element in the ring buffer.
        /// </summary>
        private int _bufferEnd;
        /// <summary>
        /// Tells if the ring buffer is full. If this is true the ring buffer will start to overwrite
        /// its values from the beginning.
        /// </summary>
        private bool _filled;

        internal VoxelCmdBuffer()
        {
            _removedVoxelBuffer = new NativeArray<VoxelCmdData>(BufferSize, Allocator.Persistent);
            _bufferEnd = 0;
            _filled = false;
        }

        /// <summary>
        /// Writes a value at the next position in the ring buffer.
        /// </summary>
        /// <param name="voxelCmdData">The data to be written.</param>
        internal void Write(VoxelCmdData voxelCmdData)
        {
            // ring buffer is full. A wrap occurs.
            if (_bufferEnd == BufferSize)
            {
                _bufferEnd = 0;
                _filled = true;
            }
            
            _removedVoxelBuffer[_bufferEnd] = voxelCmdData;
            _bufferEnd++;
        }

        internal void Clear()
        {
            _removedVoxelBuffer.Dispose();
            _removedVoxelBuffer = new NativeArray<VoxelCmdData>(BufferSize, Allocator.Persistent);
            _bufferEnd = 0;
            _filled = false;
        }

        internal void CleanUp()
        {
            _removedVoxelBuffer.Dispose();
        }

        /// <summary>
        /// Returns the amount of elements in the buffer.
        /// </summary>
        /// <returns>The amount of elements in the buffer.</returns>
        internal int GetSize()
        {
            return _filled ? _removedVoxelBuffer.Length : _bufferEnd;
        }

        internal Enumerator GetBuffer()
        {
            return new Enumerator(_removedVoxelBuffer, _filled, _bufferEnd);
        }

        /// <summary>
        /// Helps to iterator over the ring buffer in reverse order. With this implementation you can simply
        /// iterator over the VoxelCmdBuffer in reverse order with a foreach loop.
        /// </summary>
        internal struct Enumerator : IEnumerator
        {
            private NativeArray<VoxelCmdData> _buffer;
            private readonly bool _filled;
            private readonly int _bufferEnd;
            private int _iter;
            private bool _iterWrapped;

            public Enumerator(NativeArray<VoxelCmdData> buffer, bool filled, int bufferEnd)
            {
                _buffer = buffer;
                _filled = filled;
                _bufferEnd = bufferEnd;
                _iter = bufferEnd;
                _iterWrapped = false;
            }
            
            /// <summary>
            /// Has to be here so you can use the Enumerator for a foreach loop without implementing IEnumerable.
            /// </summary>
            /// <returns></returns>
            public Enumerator GetEnumerator()
            {
                return this;
            }
            
            /// <summary>
            /// Moves the iterator one step further.
            /// </summary>
            /// <returns>The information if the iterator came to an end.</returns>
            public bool MoveNext()
            {
                // reverse traversal of the buffer since we want to get voxels from the newest added to the oldest added for the undo process
                _iter--;
                // the iterator has to traverse over the edge when the buffer is filled
                if (_filled)
                {
                    if (_iter <= _bufferEnd - 1 && _iterWrapped)
                    {
                        // the iterator closed the traversal loop. Iteration finished.
                        return false;
                    }
                    if (_iter == -1)
                    {
                        // iterator is not done yet and came to the edge of the buffer. Iterator has to wrap to the ending.
                        _iter = _buffer.Length - 1;
                        _iterWrapped = true;
                    }
                }
                else
                {
                    // iterator is not done yet. Since the ring buffer is not filled a check for the beginning edge is enough.
                    if (_iter <= -1) return false;
                }

                // iterator is done
                return true;
            }

            /// <summary>
            /// Resets the iterator.
            /// </summary>
            public void Reset()
            {
                _iter = _bufferEnd;
                _iterWrapped = false;
            }

            public VoxelCmdData Current => _buffer[_iter];

            object IEnumerator.Current => Current;
        }
    }
}
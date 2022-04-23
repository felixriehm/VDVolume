using System;
using System.Threading;

namespace VDVolume.Serialization
{
    /// <summary>
    /// This class serves as an object which helps to observe an async serialization operation (loading or saving a volume).
    /// </summary>
    internal class AsyncSerializationOperation
    {
        /// <summary>
        /// This is true when the async serialization operation is finished.
        /// </summary>
        internal bool IsDone;
        /// The progress is 0.0f when the async operations is starting and 1.0f when the operation is finished.
        /// <summary>
        /// This shows the progress of the async serialization operation.
        /// </summary>
        internal float Progress;
        /// This is needed in replacement of a 'OnFinished' event since the 'OnFinished' event could trigger before
        /// the a event listener was added.
        /// <summary>
        /// Reference to the executing thread.
        /// </summary>
        internal Thread Thread;
    }
}
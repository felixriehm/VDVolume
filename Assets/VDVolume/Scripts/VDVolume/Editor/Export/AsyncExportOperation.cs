using System.Threading;

namespace VDVolume.Editor.Export
{
    public class AsyncExportOperation
    {
        /// <summary>
        /// This is true when the async export operation is finished.
        /// </summary>
        internal bool IsDone;
        /// The progress is 0.0f when the async operations is starting and 1.0f when the operation is finished.
        /// <summary>
        /// This shows the progress of the async export operation.
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
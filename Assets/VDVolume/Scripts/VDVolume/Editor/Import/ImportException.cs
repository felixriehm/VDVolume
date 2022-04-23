using System;

namespace VDVolume.Editor.Import
{
    internal class ImportException
    {
        internal class InvalidImageDimensionException : Exception
        {
            internal InvalidImageDimensionException(string msg) : base(msg) { }
        }
        
        internal class OrderedImageComparerException : Exception
        {
            internal OrderedImageComparerException(string msg) : base(msg) { }
        }
        
        internal class InvalidImageFormatException : Exception
        {
            internal InvalidImageFormatException(string msg) : base(msg) { }
        }
    }
}
using System;
using System.IO;

namespace StaticHtmlGenerator.IO {
	public static class Directories {
		public static void CopyAll(DirectoryInfo source, DirectoryInfo destination) {
			ArgumentNullException.ThrowIfNull(source, nameof(source));
			ArgumentNullException.ThrowIfNull(destination, nameof(destination));
			if( source.FullName.ToLower() == destination.FullName.ToLower() )
				return;
			if( !Directory.Exists(destination.FullName) )
				Directory.CreateDirectory(destination.FullName);
			foreach( FileInfo fileInfo in source.GetFiles() )
				fileInfo.CopyTo(Path.Combine(destination.ToString(), fileInfo.Name), overwrite: true);
			foreach( DirectoryInfo nextSource in source.GetDirectories() ) {
				DirectoryInfo nextDestination = destination.CreateSubdirectory(nextSource.Name);
				CopyAll(nextSource, nextDestination);
			}
		}
	}
}
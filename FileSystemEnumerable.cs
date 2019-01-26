// This code is public domain - from the StackOverflow post at
// <https://stackoverflow.com/questions/13130052/directoryinfo-enumeratefiles-causes-unauthorizedaccessexception-and-other>
// Provided by Matthew Brubaker.  I do not own this code in any way; I am merely putting this here to facilitate
// access by the public/improvements.

// CHANGELOG
//
// 26 Jan 2019 (Brian Hart): Added two overloads of a static Search() method so that you do not have to call
// the constructor directly.  Helpful for cleaner syntax in loops.
//
public class FileSystemEnumerable : IEnumerable<FileSystemInfo>
{
	private ILog _logger = LogManager.GetLogger(typeof(FileSystemEnumerable));

	private readonly DirectoryInfo _root;
	private readonly IList<string> _patterns;
	private readonly SearchOption _option;

	public static IEnumerable<FileSystemInfo> Search(DirectoryInfo root, string pattern = "*", SearchOption option = SearchOption.AllDirectories)
	{
		if (!root.Exists)
			throw new DirectoryNotFoundException($"The folder '{root.FullName}' could not be located.");
			
		if (string.IsNullOrWhiteSpace(pattern))
			pattern = "*";
			
		return new FileSystemEnumerable(root, pattern, option); 
	}

	public static IEnumerable<FileSystemInfo> Search(string root, string pattern = "*", SearchOption option = SearchOption.AllDirectories)
	{
		if (!Directory.Exists(root))
			throw new DirectoryNotFoundException($"The folder '{root}' could not be located.");

		if (string.IsNullOrWhiteSpace(pattern))
			pattern = "*";

		return new FileSystemEnumerable(new DirectoryInfo(root), pattern, option);
	}

	public FileSystemEnumerable(DirectoryInfo root, string pattern, SearchOption option)
	{
		_root = root;
		_patterns = new List<string> { pattern };
		_option = option;
	}

	public FileSystemEnumerable(DirectoryInfo root, IList<string> patterns, SearchOption option)
	{
		_root = root;
		_patterns = patterns;
		_option = option;
	}

	public IEnumerator<FileSystemInfo> GetEnumerator()
	{
		if (_root == null || !_root.Exists) yield break;

		IEnumerable<FileSystemInfo> matches = new List<FileSystemInfo>();
		try
		{
			_logger.DebugFormat("Attempting to enumerate '{0}'", _root.FullName);
			foreach (var pattern in _patterns)
			{
				_logger.DebugFormat("Using pattern '{0}'", pattern);
				matches = matches.Concat(_root.EnumerateDirectories(pattern, SearchOption.TopDirectoryOnly))
								 .Concat(_root.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly));
			}
		}
		catch (UnauthorizedAccessException)
		{
			_logger.WarnFormat("Unable to access '{0}'. Skipping...", _root.FullName);
			yield break;
		}
		catch (PathTooLongException ptle)
		{
			_logger.Warn(string.Format(@"Could not process path '{0}\{1}'.", _root.Parent.FullName, _root.Name), ptle);
			yield break;
		}
		catch (System.IO.IOException e)
		{
			// "The symbolic link cannot be followed because its type is disabled."
			// "The specified network name is no longer available."
			_logger.Warn(string.Format(@"Could not process path (check SymlinkEvaluation rules)'{0}\{1}'.", _root.Parent.FullName, _root.Name), e);
			yield break;
		}


		_logger.DebugFormat("Returning all objects that match the pattern(s) '{0}'", string.Join(",", _patterns));
		foreach (var file in matches)
		{
			yield return file;
		}

		if (_option == SearchOption.AllDirectories)
		{
			_logger.DebugFormat("Enumerating all child directories.");
			foreach (var dir in _root.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
			{
				_logger.DebugFormat("Enumerating '{0}'", dir.FullName);
				var fileSystemInfos = new FileSystemEnumerable(dir, _patterns, _option);
				foreach (var match in fileSystemInfos)
				{
					yield return match;
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

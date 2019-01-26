# FileSystemEnumerable
C# class that enumerates files in a folder search without terminating when exceptions occur

# Introduction
This class was initially posted to StackOverflow.com as an answer to [StackOverflow](https://stackoverflow.com/questions/13130052/directoryinfo-enumeratefiles-causes-unauthorizedaccessexception-and-other).  I posted the code to facilitate broader access by the community.
It searches a directory tree, either recursively or just the top level, and fetches the file system information for all the files and that it finds.  If an exception is thrown along the way, either because of access denied or some other cause, the file is skipped and the enumration continues. 
This is to work around the rather annoying behavior of, e.g., ```Directory.EnumerateFiles``` etc, which give up the first time they encounter an exception.

# Changelog
26 Jan 2019    Brian Hart    
* Added two overloads of a static ```Search``` method so that you can use it in a ```foreach``` in a more fluent way.  
* Test for the non-existence of the root directory and also I set defaults for the pattern and option parameters.

The usage is below (note that this is excerpted from the Stack Overflow post linked above):
```
var root = new DirectoryInfo(@"c:\wherever");
var searchPattern = @"*.txt";
var searchOption = SearchOption.AllDirectories;
var enumerable = new FileSystemEnumerable(root, searchPattern, searchOption);
```
One might also use it thus:
```
foreach (var fileSystemInfo in FileSystemEnumerable.Search(@"C:\wherever"))
{
    // ...
}
```
or:
```
foreach (var fileSystemInfo in FileSystemEnumerable.Search(new DirectoryInfo(@"C:\wherever"),
    "*.txt", SearchOption.TopDirectoryOnly))
{
    // ...
}
```
This is using the new, static ```Search``` method.

using System.Diagnostics;
using System.IO;
using NAppUpdate.Framework.Utils;

namespace FeedBuilder
{
  public class FileInfoEx
  {
    private readonly FileInfo _myFileInfo;
    private readonly string _myFileVersion;
    private readonly string _myHash;

    public FileInfo FileInfo
    {
      get { return _myFileInfo; }
    }

    public string FileVersion
    {
      get { return _myFileVersion; }
    }

    public string Hash
    {
      get { return _myHash; }
    }

    public string RelativeName { get; private set; }

    public FileInfoEx(string fileName, int rootDirLength)
    {
      _myFileInfo = new FileInfo(fileName);
      var verInfo = FileVersionInfo.GetVersionInfo(fileName);
      if (_myFileVersion != null)
        _myFileVersion = new System.Version(verInfo.FileMajorPart, verInfo.FileMinorPart, verInfo.FileBuildPart, verInfo.FilePrivatePart).ToString();
      _myHash = FileChecksum.GetSHA256Checksum(fileName);
      RelativeName = fileName.Substring(rootDirLength + 1);
    }
  }
}

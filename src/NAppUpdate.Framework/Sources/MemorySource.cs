using System;
using System.Collections.Generic;
using NAppUpdate.Framework.Common;

namespace NAppUpdate.Framework.Sources
{
  public class MemorySource : IUpdateSource
  {
    private readonly Dictionary<Uri, string> _tempFiles;

    public MemorySource(string feedString)
    {
      Feed = feedString;
      _tempFiles = new Dictionary<Uri, string>();
    }

    public string Feed { get; set; }

    public void AddTempFile(Uri uri, string path)
    {
      _tempFiles.Add(uri, path);
    }

    public string GetUpdatesFeed()
    {
      return Feed;
    }

    public bool GetData(string filePath, string basePath, Action<UpdateProgressInfo> onProgress, ref string tempFile)
    {
      Uri uriKey = null;

      if (Uri.IsWellFormedUriString(filePath, UriKind.Absolute))
        uriKey = new Uri(filePath);
      else if (Uri.IsWellFormedUriString(basePath, UriKind.Absolute))
        uriKey = new Uri(new Uri(basePath, UriKind.Absolute), filePath);

      if (uriKey == null || !_tempFiles.ContainsKey(uriKey))
        return false;

      tempFile = _tempFiles[uriKey];

      return true;
    }
  }
}

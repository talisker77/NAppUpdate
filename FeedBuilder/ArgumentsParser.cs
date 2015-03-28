using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FeedBuilder
{
  public class ArgumentsParser
  {
    public bool HasArgs { get; set; }
    public string FileName { get; set; }
    public bool ShowGui { get; set; }
    public bool Build { get; set; }
    public bool OpenOutputsFolder { get; set; }

    public ArgumentsParser(IEnumerable<string> args)
    {
      foreach (var thisArg in args)
      {
        if (thisArg.ToLower() == Application.ExecutablePath.ToLower() || thisArg.ToLower().Contains(".vshost.exe")) continue;

        var arg = CleanArg(thisArg);
        switch (arg)
        {
          case "build":
            Build = true;
            HasArgs = true;
            break;
          case "showgui":
            ShowGui = true;
            HasArgs = true;
            break;
          case "openoutputs":
            OpenOutputsFolder = true;
            HasArgs = true;
            break;
          default:
            if (IsValidFileName(thisArg))
            {
              // keep the same character casing as we were originally provided
              FileName = thisArg;
              HasArgs = true;
            }
            else Console.WriteLine("Unrecognized arg '{0}'", arg);
            break;
        }
      }
    }

    // this merely checks whether the parent folder exists and if it does, 
    // we say the filename is valid
    private static bool IsValidFileName(string filename)
    {
      if (File.Exists(filename)) return true;
      try
      {
        // the URI test... filter out things that aren't even trying to look like filenames
        // ReSharper disable UnusedVariable
        var u = new Uri(filename);
        // ReSharper restore UnusedVariable
        // see if the arg's parent folder exists
        var d = Directory.GetParent(filename);
        if (d.Exists) return true;
      }
      catch { }
      return false;
    }

    private static string CleanArg(string arg)
    {
      const string pattern1 = "^(.*)([=,:](true|0))";
      arg = arg.ToLower();
      if (arg.StartsWith("-") || arg.StartsWith("/")) arg = arg.Substring(1);
      var r = new Regex(pattern1);
      arg = r.Replace(arg, "{$1}");
      return arg;
    }
  }
}
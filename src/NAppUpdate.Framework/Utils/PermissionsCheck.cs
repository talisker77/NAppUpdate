using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace NAppUpdate.Framework.Utils
{
  public static class PermissionsCheck
  {
    private static readonly IdentityReferenceCollection Groups = WindowsIdentity.GetCurrent().Groups;
    private static readonly string SidCurrentUser = WindowsIdentity.GetCurrent().User.Value;

    public static bool IsDirectory(string path)
    {
      if (!Directory.Exists(path)) return false;
      var attr = File.GetAttributes(path);
      return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
    }

    public static bool HaveWritePermissionsForFolder(string path)
    {
      var folder = IsDirectory(path) ? path : Path.GetDirectoryName(path);
      return HaveWritePermissionsForFileOrFolder(folder);
    }

    public static bool HaveWritePermissionsForFileOrFolder(string path)
    {
      var rules = Directory.GetAccessControl(path).GetAccessRules(true, true, typeof(SecurityIdentifier));

      bool allowwrite = false, denywrite = false;
      foreach (FileSystemAccessRule rule in rules)
      {
        if (rule.AccessControlType == AccessControlType.Deny &&
            (rule.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData &&
            (Groups.Contains(rule.IdentityReference) || rule.IdentityReference.Value == SidCurrentUser)
            )
        {
          denywrite = true;
        }
        if (rule.AccessControlType == AccessControlType.Allow &&
            (rule.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData &&
            (Groups.Contains(rule.IdentityReference) || rule.IdentityReference.Value == SidCurrentUser)
            )
        {
          allowwrite = true;
        }
      }

      // If we have both allow and deny permissions, the deny takes precident.
      return allowwrite && !denywrite;
    }
  }
}

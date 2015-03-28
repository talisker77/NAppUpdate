using System;

namespace NAppUpdate.Framework.Tasks
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public class UpdateTaskAliasAttribute : Attribute
  {
    private readonly string _alias;

    public UpdateTaskAliasAttribute(string alias)
    {
      _alias = alias;
    }

    public string Alias
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _alias; }
    }
  }
}

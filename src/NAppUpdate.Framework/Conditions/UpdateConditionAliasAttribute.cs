using System;

namespace NAppUpdate.Framework.Conditions
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public class UpdateConditionAliasAttribute : Attribute
  {
    private readonly string _alias;

    public UpdateConditionAliasAttribute(string alias)
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

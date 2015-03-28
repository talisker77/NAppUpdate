using System;
using System.Collections.Generic;
using System.Reflection;

namespace NAppUpdate.Framework.Utils
{
  using Tasks;
  using Conditions;
  using Common;

  public static class Reflection
  {
    internal static void FindTasksAndConditionsInAssembly(Assembly assembly,
        Dictionary<string, Type> updateTasks, Dictionary<string, Type> updateConditions)
    {
      foreach (var t in assembly.GetTypes())
      {
        if (typeof(IUpdateTask).IsAssignableFrom(t))
        {
          updateTasks.Add(t.Name, t);
          var tasksAliases = (UpdateTaskAliasAttribute[])t.GetCustomAttributes(typeof(UpdateTaskAliasAttribute), false);
          foreach (var alias in tasksAliases)
          {
            updateTasks.Add(alias.Alias, t);
          }
        }
        else if (typeof(IUpdateCondition).IsAssignableFrom(t))
        {
          updateConditions.Add(t.Name, t);
          var tasksAliases = (UpdateConditionAliasAttribute[])t.GetCustomAttributes(typeof(UpdateConditionAliasAttribute), false);
          foreach (var alias in tasksAliases)
          {
            updateConditions.Add(alias.Alias, t);
          }
        }
      }
    }

    internal static void SetNauAttributes(INauFieldsHolder fieldsHolder, Dictionary<string, string> attributes)
    {
      // Load public non-static properties
      var propertyInfos = fieldsHolder.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

      foreach (var pi in propertyInfos)
      {
        var atts = pi.GetCustomAttributes(typeof(NauFieldAttribute), false);
        if (atts.Length != 1) continue; // NauFieldAttribute doesn't allow multiples

        var nfa = (NauFieldAttribute)atts[0];

        // Get the attribute value, process it, and set the object's property with that value
        string attValue;
        if (!attributes.TryGetValue(nfa.Alias, out attValue)) continue;
        if (pi.PropertyType == typeof(String))
        {
          pi.SetValue(fieldsHolder, attValue, null);
        }
        else if (pi.PropertyType == typeof(DateTime))
        {
          DateTime dt;
          long filetime;
          if (DateTime.TryParse(attValue, out dt))
            pi.SetValue(fieldsHolder, dt, null);
          else if (long.TryParse(attValue, out filetime))
          {
            try
            {
              // use local time, not UTC
              dt = DateTime.FromFileTime(filetime);
              pi.SetValue(fieldsHolder, dt, null);
            }
            catch { }
          }
        }
        // TODO: type: Uri
        else if (pi.PropertyType.IsEnum)
        {
          var eObj = Enum.Parse(pi.PropertyType, attValue);
          if (eObj != null)
            pi.SetValue(fieldsHolder, eObj, null);
        }
        else
        {
          var mi = pi.PropertyType.GetMethod("Parse", new[] { typeof(String) });
          if (mi == null) continue;
          var o = mi.Invoke(null, new object[] { attValue });

          if (o != null)
            pi.SetValue(fieldsHolder, o, null);
        }
      }
    }

    internal static object GetNauAttribute(INauFieldsHolder fieldsHolder, string attributeName)
    {
      var pi = fieldsHolder.GetType().GetProperty(attributeName, BindingFlags.Public | BindingFlags.Instance);
      return pi == null ? null : pi.GetValue(fieldsHolder, null);
    }
  }
}

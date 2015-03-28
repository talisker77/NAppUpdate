﻿using System;
using System.Collections.Generic;
using System.Xml;

using NAppUpdate.Framework.Tasks;
using NAppUpdate.Framework.Conditions;

namespace NAppUpdate.Framework.FeedReaders
{
  public class NauXmlFeedReader : IUpdateFeedReader
  {
    private Dictionary<string, Type> UpdateConditions { get; set; }
    private Dictionary<string, Type> UpdateTasks { get; set; }

    public IList<IUpdateTask> Read(string feed)
    {
      // Lazy-load the Condition and Task objects contained in this assembly, unless some have already
      // been loaded (by a previous lazy-loading in a call to Read, or by an explicit loading)
      if (UpdateTasks == null)
      {
        UpdateConditions = new Dictionary<string, Type>();
        UpdateTasks = new Dictionary<string, Type>();
        Utils.Reflection.FindTasksAndConditionsInAssembly(this.GetType().Assembly, UpdateTasks, UpdateConditions);
      }

      var ret = new List<IUpdateTask>();

      var doc = new XmlDocument();
      doc.LoadXml(feed);

      // Support for different feed versions
      var root = doc.SelectSingleNode(@"/Feed[version=""1.0""] | /Feed") ?? doc;

      if (root.Attributes["BaseUrl"] != null && !string.IsNullOrEmpty(root.Attributes["BaseUrl"].Value))
        UpdateManager.Instance.BaseUrl = root.Attributes["BaseUrl"].Value;

      // Temporary collection of attributes, used to aggregate them all with their values
      // to reduce Reflection calls
      var attributes = new Dictionary<string, string>();

      var nl = root.SelectNodes("./Tasks/*");
      if (nl == null) return new List<IUpdateTask>(); // TODO: wrong format, probably should throw exception
      foreach (XmlNode node in nl)
      {
        // Find the requested task type and create a new instance of it
        if (!UpdateTasks.ContainsKey(node.Name))
          continue;

        var task = (IUpdateTask)Activator.CreateInstance(UpdateTasks[node.Name]);

        // Store all other task attributes, to be used by the task object later
        if (node.Attributes != null)
        {
          foreach (XmlAttribute att in node.Attributes)
          {
            if ("type".Equals(att.Name))
              continue;

            attributes.Add(att.Name, att.Value);
          }
          if (attributes.Count > 0)
          {
            Utils.Reflection.SetNauAttributes(task, attributes);
            attributes.Clear();
          }
          // TODO: Check to see if all required task fields have been set
        }

        if (node.HasChildNodes)
        {
          if (node["Description"] != null)
            task.Description = node["Description"].InnerText;

          // Read update conditions
          if (node["Conditions"] != null)
          {
            var conditionObject = ReadCondition(node["Conditions"]);
            if (conditionObject != null)
            {
              var boolCond = conditionObject as BooleanCondition;
              if (boolCond != null)
                task.UpdateConditions = boolCond;
              else
              {
                if (task.UpdateConditions == null) task.UpdateConditions = new BooleanCondition();
                task.UpdateConditions.AddCondition(conditionObject);
              }
            }
          }
        }

        ret.Add(task);
      }
      return ret;
    }

    private IUpdateCondition ReadCondition(XmlNode cnd)
    {
      IUpdateCondition conditionObject = null;
      if (cnd.ChildNodes.Count > 0 || "GroupCondition".Equals(cnd.Name))
      {
        var bc = new BooleanCondition();
        foreach (XmlNode child in cnd.ChildNodes)
        {
          var childCondition = ReadCondition(child);
          if (childCondition != null)
            bc.AddCondition(childCondition,
                            BooleanCondition.ConditionTypeFromString(child.Attributes != null && child.Attributes["type"] != null
                                                                         ? child.Attributes["type"].Value : null));
        }
        if (bc.ChildConditionsCount > 0)
          conditionObject = bc.Degrade();
      }
      else if (UpdateConditions.ContainsKey(cnd.Name))
      {
        conditionObject = (IUpdateCondition)Activator.CreateInstance(UpdateConditions[cnd.Name]);

        if (cnd.Attributes != null)
        {
          var dict = new Dictionary<string, string>();

          // Store all other attributes, to be used by the condition object later
          foreach (XmlAttribute att in cnd.Attributes)
          {
            if ("type".Equals(att.Name))
              continue;

            dict.Add(att.Name, att.Value);
          }
          if (dict.Count > 0)
            Utils.Reflection.SetNauAttributes(conditionObject, dict);
        }
      }
      return conditionObject;
    }

    public void LoadConditionsAndTasks(System.Reflection.Assembly assembly)
    {
      if (UpdateTasks == null)
      {
        UpdateConditions = new Dictionary<string, Type>();
        UpdateTasks = new Dictionary<string, Type>();
      }
      Utils.Reflection.FindTasksAndConditionsInAssembly(assembly, UpdateTasks, UpdateConditions);
    }
  }
}

using System.Xml;
using System.Collections.Generic;
using NAppUpdate.Framework.Tasks;
using NAppUpdate.Framework.Conditions;

namespace NAppUpdate.Framework.FeedReaders
{
  public class AppcastReader : IUpdateFeedReader
  {
    // http://learn.adobe.com/wiki/display/ADCdocs/Appcasting+RSS

    public IList<IUpdateTask> Read(string feed)
    {
      var doc = new XmlDocument();
      doc.LoadXml(feed);
      var nl = doc.SelectNodes("/rss/channel/item");

      var ret = new List<IUpdateTask>();

      foreach (XmlNode n in nl)
      {
        var task = new FileUpdateTask
        {
          Description = n["description"].InnerText,
          UpdateTo = n["enclosure"].Attributes["url"].Value
        };

        var cnd = new FileVersionCondition
        {
          Version = n["appcast:version"].InnerText
        };
        if (task.UpdateConditions == null) task.UpdateConditions = new BooleanCondition();
        task.UpdateConditions.AddCondition(cnd, BooleanCondition.ConditionType.AND);

        ret.Add(task);
      }

      return ret;
    }
  }
}
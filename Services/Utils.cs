using HtmlAgilityPack;
using System;
using System.Linq;

public class Utils
{
    public static string HtmlCleaner(string htmlBody)
    {
        if (htmlBody == null)
        {
            throw new ArgumentNullException(nameof(htmlBody));
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlBody);

        var unwantedDivs = htmlDoc.DocumentNode.SelectNodes("//div")
            ?.Where(tag =>
                tag.GetAttributeValue("class", "").ToLower().Contains("signature") ||
                tag.GetAttributeValue("id", "").Contains("divRplyFwdMsg") ||
                tag.GetAttributeValue("id", "").ToLower().Contains("signature") ||
                tag.GetAttributeValue("id", "").ToLower().Contains("appendonsend") ||
                tag.GetAttributeValue("class", "").ToLower().Contains("gmail_attr"))
            .ToList();

        if (unwantedDivs != null)
        {
            foreach (var div in unwantedDivs)
            {
                div.Remove();
            }
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }
    public static string HtmlToText(string htmlBody)
    {
        if (htmlBody == null)
        {
            throw new ArgumentNullException(nameof(htmlBody));
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlBody);
        return htmlDoc.DocumentNode.InnerText;
    }
}
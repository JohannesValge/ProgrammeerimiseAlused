using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

var feeds = new[]
{
    (Name: "Postimees", Url: "https://www.postimees.ee/rss"),
    (Name: "Lääne Elu", Url: "https://online.le.ee/feed/"),
    (Name: "Äripäev", Url: "https://feeds.feedburner.com/aripaev-rss"),
    (Name: "ERR", Url: "https://www.err.ee/rss")
};

using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
client.DefaultRequestHeaders.UserAgent.ParseAdd("SchoolRssAggregator/1.0");
var news = new List<RssItem>();

foreach (var feed in feeds)
{
    try
    {
        using var stream = await client.GetStreamAsync(feed.Url);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });
        var document = XDocument.Load(reader);
        var count = 0;
        foreach (var item in document.Descendants("item"))
        {
            var title = item.Element("title")?.Value.Trim();
            var link = item.Element("link")?.Value.Trim();
            var dateText = item.Element("pubDate")?.Value
                ?? item.Elements().FirstOrDefault(e => e.Name.LocalName == "date")?.Value;
            // Mõned RSS-vood kasutavad ajavööndi nimetust GMT.
            dateText = dateText?.Replace("GMT", "+0000", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(title)
                || !Uri.TryCreate(link, UriKind.Absolute, out var uri)
                || (uri.Scheme != "https" && uri.Scheme != "http")
                || !DateTimeOffset.TryParse(dateText, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out var published))
            {
                Console.WriteLine($"{feed.Name}: puudulike andmetega uudis jäeti vahele.");
                continue;
            }

            var description = item.Element("description")?.Value ?? "";
            // Kirjeldus kuvatakse tekstina; väljaande HTML-i ei käivitata.
            description = WebUtility.HtmlDecode(Regex.Replace(description, "<[^>]*>", " "));
            news.Add(new RssItem(title, uri.AbsoluteUri, description, feed.Name, published));
            count++;
        }
        Console.WriteLine($"{feed.Name}: loetud {count} uudist.");
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException or IOException)
    {
        Console.Error.WriteLine($"{feed.Name}: voo lugemine ebaõnnestus: {ex.Message}");
    }
}

if (news.Count == 0)
{
    Console.Error.WriteLine("Ühtegi uudist ei õnnestunud lugeda. HTML-faili ei muudeta.");
    return 1;
}

var latest = news.OrderByDescending(item => item.Published).Take(20).ToList();
var output = Path.GetFullPath("uudised.html");
using (var file = new FileStream(output, FileMode.Create, FileAccess.Write))
using (var writer = new StreamWriter(file, new UTF8Encoding(false)))
{
    writer.WriteLine("<!DOCTYPE html><html lang=\"et\"><head><meta charset=\"utf-8\">");
    writer.WriteLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>Viimased uudised</title>");
    writer.WriteLine("<style>body{font-family:Arial,sans-serif;max-width:900px;margin:40px auto;padding:0 20px;line-height:1.6}li{margin-bottom:24px}a{font-size:1.15em}time{color:#555}</style></head><body>");
    writer.WriteLine($"<h1>{latest.Count} viimati avaldatud uudist</h1><ul>");
    foreach (var item in latest)
    {
        writer.WriteLine($"<li><a href=\"{WebUtility.HtmlEncode(item.Link)}\">{WebUtility.HtmlEncode(item.Title)}</a><br>");
        writer.WriteLine($"<time datetime=\"{item.Published:O}\">{item.Published:dd.MM.yyyy HH:mm zzz}</time> | {WebUtility.HtmlEncode(item.Publisher)}<br>");
        writer.WriteLine($"{WebUtility.HtmlEncode(item.Description)}</li>");
    }
    writer.WriteLine("</ul></body></html>");
}

Console.WriteLine($"Salvestatud: {output}");
if (!args.Contains("--no-browser"))
{
    try
    {
        Process.Start(new ProcessStartInfo(output) { UseShellExecute = true });
    }
    catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
    {
        Console.Error.WriteLine($"Brauserit ei saanud avada. Ava fail käsitsi: {ex.Message}");
    }
}
return 0;

public sealed record RssItem(string Title, string Link, string Description,
    string Publisher, DateTimeOffset Published);

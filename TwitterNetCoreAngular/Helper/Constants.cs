
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
public class Constants
{
    public static String urlRules { get; set; } = "https://api.twitter.com/2/tweets/search/stream/rules";
    public static String urlStream { get; set; } = "https://api.twitter.com/2/tweets/search/stream?tweet.fields=public_metrics,created_at,source&expansions=author_id&user.fields=description,location,profile_image_url";
    public static String token { get; set; } = "";
    public static String url { get; set; } = "";
    public static String url_localhost { get; set; } = "mongodb://127.0.0.1:27017";
    public static String dbname { get; set; } = "data-warehouse";

}

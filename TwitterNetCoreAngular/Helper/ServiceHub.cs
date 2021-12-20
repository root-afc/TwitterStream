using MongoDB.Driver;
using System.Text.Json;
using System.Text;
using System.Collections;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using Microsoft.AspNetCore.SignalR;

public class ServiceHub
{
    private static String urlRules = "https://api.twitter.com/2/tweets/search/stream/rules";
    private static String urlStream = "https://api.twitter.com/2/tweets/search/stream?tweet.fields=public_metrics,created_at,source&expansions=author_id&user.fields=description,location,profile_image_url";
    private static String token = "";
    private static HttpClient? httpClient;
    private static HttpResponseMessage? httpResponse;
    private static HttpContent? httpContent;
    private readonly IMongoCollection<E_Tweet> _mongoCollection;
    private readonly IMongoCollection<BsonDocument> _mongoCollectionEnumator;
    private readonly IHubContext<TweetsHub, IHubClient> _hubContext;

    public ServiceHub(IHubContext<TweetsHub, IHubClient> hubContext)
    {
        String url = "";
        String dbname = "data-warehouse";
        String collection = "tweets";
        MongoClient client = new MongoClient(url);
        IMongoDatabase db = client.GetDatabase(dbname);
        _mongoCollectionEnumator = db.GetCollection<BsonDocument>(collection);
        _mongoCollection = db.GetCollection<E_Tweet>(collection);
        _hubContext = hubContext;
    }

    public ServiceHub()
    {
    }

    public async Task setting_Up_Rules()
    {

        try
        {
            // Getting rules
            Console.WriteLine("Getting rules...");
            Data_E_Rule data_E_Rule = await getRules();
            if (data_E_Rule.data != null)
            {
                // Deleting rules
                Console.WriteLine("Deleting rules...");
                await deleteRules(data_E_Rule);
            }
            // Posting rules
            Console.WriteLine("Posting rules...");
            await postRules();
            // Broadcast tweets and trigger notification after collection database changes
            Task task = Task.Run(() =>
           {
               streamTweets();
           });

        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<Data_E_Rule> getRules()
    {
        try
        {
            httpClient = new HttpClient();
            // setting up the headers
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            // getting the rules
            httpResponse = new HttpResponseMessage();
            httpResponse = await httpClient.GetAsync(urlRules);
            httpResponse.EnsureSuccessStatusCode();
            Data_E_Rule? responseBody = await httpResponse.Content.ReadFromJsonAsync<Data_E_Rule>();
            return responseBody!;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e.Message);
            return null!;
        }
    }

    public async Task deleteRules(Data_E_Rule responseBody)
    {
        httpClient = new HttpClient();
        // map and put ids into array
        IEnumerable idsArray = responseBody.data!.Select(rule => rule.id);
        Delete_E_Rule delete_E_Rule = new Delete_E_Rule() { delete = new Ids_E_Rule { ids = idsArray } };
        String DeleteIdsBody = JsonSerializer.Serialize(delete_E_Rule);
        // setting up the headers
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        // HttpContent with json and UTF8 encoding
        httpContent = new StringContent(DeleteIdsBody, Encoding.UTF8, "application/json");
        // deleting the rules
        httpResponse = new HttpResponseMessage();
        httpResponse = await httpClient.PostAsync(urlRules, httpContent);
        httpResponse.EnsureSuccessStatusCode();
    }
    public async Task postRules()
    {
        try
        {
            // setting up the rules
            List<E_Rule> rules_list = new List<E_Rule>();
            rules_list.Add(new E_Rule("from:willaxtv"));
            rules_list.Add(new E_Rule("from:PedroCastilloTe"));
            rules_list.Add(new E_Rule("from:canalN_"));      
            rules_list.Add(new E_Rule("from:elcomercio_peru"));

            Add_E_Rule add_E_Rule = new Add_E_Rule() { add = rules_list };
            // convert to json string
            String add_Rules = JsonSerializer.Serialize(add_E_Rule);
            // setting up the headers
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            // HttpContent [data_Rules] with json and UTF8 encoding
            httpContent = new StringContent(add_Rules, Encoding.UTF8, "application/json");
            // posting the rules
            httpResponse = new HttpResponseMessage();
            httpResponse = await httpClient.PostAsync(urlRules, httpContent);
            httpResponse.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async void streamTweets()
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        Stream stream = await httpClient.GetStreamAsync(urlStream);
        using (StreamReader reader = new StreamReader(stream))
        {
            while (!reader.EndOfStream)
            {
                try
                {
                    String? tweet_string = await reader.ReadLineAsync();
                    JObject tweet = JObject.Parse(tweet_string!);
                    E_Tweet e_Tweet = new E_Tweet()
                    {
                        text = (string)tweet["data"]!["text"]!,
                        username = (string)tweet["includes"]!["users"]![0]!["username"]!,
                        linkTweet = "https://twitter.com/" + tweet["includes"]!["users"]![0]!["username"] + "/status/" + tweet["data"]!["id"],
                        name = (string)tweet["includes"]!["users"]![0]!["name"]!,
                        description = (string)tweet["includes"]!["users"]![0]!["description"]!,
                        created_at = (DateTime)tweet["data"]!["created_at"]!,
                        source = (string)tweet["data"]!["source"]!,
                        profile_image_url = (string)tweet["includes"]!["users"]![0]!["profile_image_url"]!,
                        location = (string)tweet["includes"]!["users"]![0]!["location"]!
                    };
                    Console.WriteLine(JsonSerializer.Serialize(e_Tweet));
                    await create(e_Tweet);
                }
                catch (Exception e)
                {
                }
            }
        }

    }
    public async Task create(E_Tweet e_Tweet)
    {
        try
        {
            await _mongoCollection.InsertOneAsync(e_Tweet);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public List<E_Tweet> getAll()
    {
        try
        {
            // return _mongoCollection.Find(tweet => true).ToList();
            return _mongoCollection.Find(tweet => true).SortByDescending(tweet => tweet.created_at).ToList();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async void streamNotification()
    {
        Console.WriteLine("running streamNotification()");
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
        .Match(change => change.OperationType == ChangeStreamOperationType.Insert || change.OperationType == ChangeStreamOperationType.Update || change.OperationType == ChangeStreamOperationType.Replace)
        .AppendStage<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>, BsonDocument>(
        "{ $project: { '_id': 1, 'fullDocument': 1, 'ns': 1, 'documentKey': 1 }}");

        ChangeStreamOptions changeStreamOptions = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        };
        IEnumerator<BsonDocument> enumerator = _mongoCollectionEnumator.Watch(pipeline, changeStreamOptions).ToEnumerable().GetEnumerator();

        using (enumerator)
        {
            while (enumerator.MoveNext())
            {
                try
                {
                    Console.WriteLine("new changes...");
                    // signal notification:
                    await _hubContext.Clients.All.BroadcastMessage();
                    // Console.WriteLine(enumerator.Current);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            // enumerator.Dispose();
        }
    }
}

public interface IHubClient
{
    Task BroadcastMessage();
}

public class TweetsHub : Hub<IHubClient>
{
}
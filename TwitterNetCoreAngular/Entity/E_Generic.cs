using MongoDB.Bson;
using System.Collections;

public class E_Rule
{
    public String? id { get; set; }
    public String value { get; set; }

    public E_Rule(string value)
    {
        this.value = value;
    }
}

public class Add_E_Rule
{
    public List<E_Rule>? add { get; set; }
}

public class Delete_E_Rule
{
    public Ids_E_Rule? delete { get; set; }
}

public class Ids_E_Rule
{
    public IEnumerable? ids { get; set; }
}

public class Data_E_Rule
{
    public List<E_Rule>? data { get; set; }
}
public class E_Tweet
{
    public ObjectId Id { get; set; }
    public String? text { get; set; }
    public String? username { get; set; }
    public String? linkTweet { get; set; }
    public String? name { get; set; }
    public String? description { get; set; }
    public DateTime? created_at { get; set; }
    public String? source { get; set; }
    public String? profile_image_url { get; set; }
    public String? location { get; set; }
}
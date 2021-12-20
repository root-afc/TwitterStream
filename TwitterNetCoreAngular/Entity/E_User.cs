using MongoDB.Bson;
public class E_User
{
    public ObjectId Id { get; set; }
    public Int32 documentId { get; set; }
    public String? name { get; set; }
}
// using Amazon.DynamoDBv2.DataModel;
//
// namespace OutlayApp.Infrastructure.Database.DynamoDb;
//
// [DynamoDBTable("logos")]
// public class Logo
// {
//     [DynamoDBHashKey("id")]
//     public Guid? Id { get; set; }
//     
//     [DynamoDBProperty("logo_url")]
//     public string? LogoUrl { get; set; }
//     
//     [DynamoDBProperty("times_retrieved")]
//     public int? TimesRetrieved { get; set; }
//
//     [DynamoDBProperty("last_time_retrieved")]
//     public long LastTimeRetrieved { get; set; }
// }
//todo needDynamo?
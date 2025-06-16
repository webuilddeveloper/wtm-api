using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace cms_api.Models
{
    public class Database
    {
        public string _mongoConnection { get; set; }
        public string _databaseName { get; set; }

        public Database(string db = "wtm_prod")
        {
            this._mongoConnection = "mongodb://127.0.0.1:27017";
            //this._mongoConnection = "mongodb://adminvet:V3t321@209.15.96.238:28018";
            this._databaseName = db;
        }

        public IMongoCollection<BsonDocument> MongoClient(string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(this._databaseName);
            return db.GetCollection<BsonDocument>(collection);
        }

        public IMongoCollection<T> MongoClient<T>(string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(this._databaseName);
            return db.GetCollection<T>(collection);
        }

        public IMongoCollection<BsonDocument> MongoClient(string database, string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(_databaseName + "_" + database);
            return db.GetCollection<BsonDocument>(collection);
        }

        public IMongoCollection<T> MongoClient<T>(string database, string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(database);
            return db.GetCollection<T>(collection);
        }
    }
}

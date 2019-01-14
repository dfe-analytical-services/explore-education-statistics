const co = require("co");
const azure = require("azure-storage");
const parse = require("csv-parse");
const MongoClient = require("mongodb").MongoClient;

module.exports = async function(context, myQueueItem) {
  co(function*() {
    var blobService = azure.createBlobService();

    context.log("Publication blob id", myQueueItem);

    var client = yield MongoClient.connect(
      "mongodb://root:example@localhost:27017/test",
      { db: { authSource: "admin" } }
    );

    const db = client.db("test");
    const collection = db.collection("publications");

    blobService.createReadStream("publications", myQueueItem).pipe(
      parse({
        columns: true,
        separator: ",",
        skip_empty_lines: true,
        trim: true
      })
        .on("readable", function() {
          let record;
          while ((record = this.read())) {
            collection.insertOne(record);
          }
        })
        .on("end", function() {
          client.close();
        })
    );
  }).catch(function(err) {
    context.log(err.stack);
  });
};

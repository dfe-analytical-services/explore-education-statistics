const co = require('co');
const azure = require('azure-storage');
const parse = require('csv-parse');
const MongoClient = require('mongodb').MongoClient;

module.exports = async function(context, myQueueItem) {
  context.log('Message', myQueueItem);

  co(function*() {
    var blobService = azure.createBlobService();

    var client = yield MongoClient.connect(
      'mongodb://root:example@mongo:27017/test',
      { db: { authSource: 'admin' } },
    );

    const chunk = function(array, size) {
      if (!array.length) {
        return [];
      }
      const head = array.slice(0, size);
      const tail = array.slice(size);
      return [head, ...chunk(tail, size)];
    };

    const db = client.db('test');
    const collection = db.collection('releases');
    const output = [];

    blobService.createReadStream('releases', myQueueItem.Release).pipe(
      parse({
        columns: true,
        separator: ',',
        // eslint-disable-next-line @typescript-eslint/camelcase
        skip_empty_lines: true,
        trim: true,
      })
        .on('readable', function() {
          let record;
          while ((record = this.read())) {
            output.push(record);
          }
        })
        .on('end', function() {
          context.log(`end - records: ${output.length}`);
          for (let c of chunk(output, 5000)) {
            collection.insertMany(c);
            collection
              .find({})
              .count()
              .then(function(value) {
                context.log(`count: ${value}`);
              });
          }
          client.close();
        }),
    );
  }).catch(function(err) {
    context.log(err.stack);
  });
};

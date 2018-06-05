var request = require('request');
var async = require('async');

module.exports = function (context, req) {
    context.log('Node.js HTTP trigger function processed message');

    var text = req.body.text;

    if (!text) {
        context.log('Malformed request received; exiting');

        context.res = {
            status: 400,
            body: 'Malformed request received; exiting'
        };

        context.done();
        return;
    }

    context.log('Requests to Text Analysis APIs starting');

    async.parallel({
        entities: function (callback) {
            context.log('Starting call to Entities API');

            request.post('https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/entities', {
                json: true,
                headers: {
                    'Ocp-Apim-Subscription-Key': process.env['TEXT_ANALYTICS_KEY']
                },
                body: {
                    documents: [{
                        language: 'en',
                        id: '1',
                        text: text
                    }]
                }
            }, function (error, response, body) {
                if (error || response.statusCode !== 200) {
                    context.log('Error on request to Entities API');
                    context.log('Error object: %j', error);

                    callback(error);
                    return;
                }

                context.log('Request to Entities API returned');
                context.log(body);

                callback(null, body);
            });
        },
        keyPhrases: function (callback) {
            context.log('Starting call to Key Phrases API');

            request.post('https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases', {
                json: true,
                headers: {
                    'Ocp-Apim-Subscription-Key': process.env['TEXT_ANALYTICS_KEY']
                },
                body: {
                    documents: [{
                        language: 'en',
                        id: '1',
                        text: text
                    }]
                }
            }, function (error, response, body) {
                if (error || response.statusCode !== 200) {
                    context.log('Error on request to Key Phrases API');
                    context.log('Error object: %j', error);

                    callback(error);
                    return;
                }

                context.log('Request to Key Phrases API returned');
                context.log(body);

                callback(null, body);
            });
        },
        sentiment: function (callback) {
            context.log('Starting call to Key Phrases API');

            request.post('https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment', {
                json: true,
                headers: {
                    'Ocp-Apim-Subscription-Key': process.env['TEXT_ANALYTICS_KEY']
                },
                body: {
                    documents: [{
                        language: 'en',
                        id: '1',
                        text: text
                    }]
                }
            }, function (error, response, body) {
                if (error || response.statusCode !== 200) {
                    context.log('Error on request to Sentiment API');
                    context.log('Error object: %j', error);

                    callback(error);
                    return;
                }

                context.log('Request to Sentiment API returned');
                context.log(body);

                callback(null, body);
            });
        }
    }, function (err, results) {
        context.log('Requests to Text Analysis APIs complete');

        if (err) {
            context.log('Error returned from at least one call');
            context.log(err);

            context.res = {
                status: 400,
                body: 'Error returned from at least one call received; exiting'
            };
        } else {
            context.res = {
                body: results
            }
        }

        var output = {
            type: 'text',
            text: text
        };

        context.done(null, output);
    });
};

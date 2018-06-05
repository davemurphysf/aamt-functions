var request = require('request');

module.exports = function (context, req) {
    context.log('Node.js HTTP trigger function processed message');

    var name = req.query.username;
    var count = parseInt(req.query.count, 10) || 20;

    if (!name || !count) {
        context.res = {
            status: 400,
            body: 'username or count is invalid'
        };
        console.log('username or count is invalid');
        context.done();
        return;
    }

    context.log('Received request for Tweets for user: %s', name);

    context.log('Fetching oauth token');

    request.post('https://api.twitter.com/oauth2/token', {
        auth: {
            'user': process.env['TWITTER_CONSUMER_KEY'],
            'password': process.env['TWITTER_CONSUMER_SECRET']
        },
        form: {
            'grant_type': 'client_credentials'
        },
        gzip: true
    },
        function (error, response, body) {
            if (error || response.statusCode !== 200) {
                context.log('Error on request to Twitter for oauth token');
                context.log('Error object: %j', error);

                context.res = {
                    status: 400,
                    body: 'Error on request to Twitter for oauth token'
                };

                context.done();
                return;
            }
            context.log('Request to Twitter for oauth token returned');

            var parsedBody = JSON.parse(response.body);

            if (!parsedBody || !parsedBody.access_token) {
                context.log('Parsing response body failed; exiting');
                context.log('Response object: %j', response);

                context.res = {
                    status: 400,
                    body: 'Parsing response body failed; exiting'
                };

                context.done();
                return;
            }

            context.log('Fetching tweets for user: %s', name);

            request.get('https://api.twitter.com/1.1/statuses/user_timeline.json?count=' + count + '&screen_name=' + name, {
                'auth': {
                    'bearer': parsedBody.access_token
                },
                gzip: true
            }, function (error, response, body) {
                if (error || response.statusCode !== 200) {
                    context.log('Error on request to Twitter for tweets');
                    context.log('Error object: %j', error);

                    context.res = {
                        status: 400,
                        body: error.message || "Error parsing tweets"
                    };

                    context.done();
                    return;
                }

                context.log('Request to Twitter for tweets returned');

                context.res = {
                    body: body
                };

                var output = {
                    username: name,
                    count: count
                };

                context.done(null, output);
            });
        }
    );
};

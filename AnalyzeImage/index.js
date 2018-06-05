var request = require('request');

module.exports = function (context, req) {
    context.log('Node.js HTTP trigger function processed message');
    var imageUrl = req.body.imageUrl;

    if (!imageUrl) {
        context.log('Malformed request received; exiting');

        context.res = {
            status: 400,
            body: 'Malformed request received; exiting'
        };

        context.done();
        return;
    }

    context.log('Requests to Image Analysis API starting');

    request.post('https://westus2.api.cognitive.microsoft.com/vision/v2.0/analyze', {
        json: true,
        headers: {
            'Ocp-Apim-Subscription-Key': process.env['COMPUTER_VISION_KEY']
        },
        qs: {
            visualFeatures: "Categories,Description,Color,Tags,Faces,Adult",
            details: "Celebrities,Landmarks",
            language: "en"
        },
        body: {
            url: imageUrl
        }
    }, function (error, response, body) {
        if (error || response.statusCode !== 200) {
            context.log('Error on request to Computer Vision API');
            context.log('Error object: %j', error);

            context.res = {
                status: 400,
                body: 'Error returned from Computer Vision API call received; exiting'
            };

            context.done();
            return;
        }

        context.log('Request to Computer Vision API returned');
        context.log(body);

        context.res = {
            body: body
        }

        var output = {
            type: 'image',
            text: imageUrl
        };

        context.done(null, output);
    });
};
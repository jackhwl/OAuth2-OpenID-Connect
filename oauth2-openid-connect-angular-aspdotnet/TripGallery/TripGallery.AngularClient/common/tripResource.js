(function () {
    "use strict";

    angular
        .module("common.services")
        .factory("tripResource",
                ["$resource",
                 "appSettings", "tokenContainer",  
                    tripResource])

    function tripResource($resource, appSettings, tokenContainer) {
         return $resource(appSettings.tripGalleryAPI + "/api/trips/:tripId", null,
             {
                 'query': {
                     isArray: true,
                     headers: { 'Authorization': 'Bearer ' + tokenContainer.getToken().token }
                 },
                'patch':
                    { 
                        method: 'PATCH',
                        transformRequest: createJsonPatchDocument 
                    }

            });
    };

    var createJsonPatchDocument = function (data) {
         
        // create a JsonPatchDocument for the resource - the only
        // thing that can be updated in this specific case is the
        // isPublic boolean. 

        var dataToSend = "[{op: 'replace', path: '/isPublic', value: '" + !data["isPublic"] + "'}]";
        return dataToSend;
    }

}());


"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var api_1 = require("@common/services/api");
exports.default = {
    getPermalink: function (publicationSlug) {
        return api_1.dataApi.get("Permalink/" + publicationSlug);
    },
    createTablePermalink: function (query) {
        return api_1.dataApi.post('/permalink', query);
    },
};

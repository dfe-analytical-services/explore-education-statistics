"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var api_1 = require("@common/services/api");
exports.default = {
    getThemes: function () {
        return api_1.dataApi.get("/meta/themes");
    },
    getPublicationMeta: function (publicationUuid) {
        return api_1.dataApi.get("/meta/publication/" + publicationUuid);
    },
    getPublicationSubjectMeta: function (subjectId) {
        return api_1.dataApi.get("/meta/subject/" + subjectId);
    },
    filterPublicationSubjectMeta: function (query) {
        return api_1.dataApi.post('/meta/subject', query);
    },
    getTableData: function (query) {
        return api_1.dataApi.post('/tablebuilder', query);
    },
};

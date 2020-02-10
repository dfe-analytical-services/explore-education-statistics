export default {
  createPublication: {
    route: '/theme/:themeId/topic/:topicId/create-publication',
    generateLink: (themeId: string, topicId: string) =>
      `/theme/${themeId}/topic/${topicId}/create-publication`,
  },
};

export default {
  createPublication: {
    route: '/topic/:topicId/publication/create',
    generateLink: (topicId: string) => `/topic/${topicId}/publication/create`,
  },
};

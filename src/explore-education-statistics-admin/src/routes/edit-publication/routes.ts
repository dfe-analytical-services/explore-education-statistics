export default {
  createPublication: {
    route: '/topic/:topicId/create-publication',
    generateLink: (topicId: string) => `/topic/${topicId}/create-publication`,
  }
}
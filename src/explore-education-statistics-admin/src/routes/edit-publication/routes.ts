import { generatePath } from 'react-router';

const route = '/theme/:themeId/topic/:topicId/create-publication';

export default {
  createPublication: {
    route,
    generateLink: (themeId: string, topicId: string) =>
      generatePath(route, {
        themeId,
        topicId,
      }),
  },
};

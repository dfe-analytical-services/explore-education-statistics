export const generateAdminDashboardThemeTopicLink: (
  themeId: string,
  topicId: string,
) => string = (themeId, topicId) => `/dashboard/${themeId}/${topicId}`;

export default {
  adminDashboard: '/dashboard',
  adminDashboardThemeTopic: '/dashboard/:themeId/:topicId',
};

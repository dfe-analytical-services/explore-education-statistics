export default {
  adminDashboard: '/dashboard',
  adminDashboardThemeTopic: '/dashboard/:themeId/:topicId',
  generateLink: (themeId: string, topicId: string) =>
    `/dashboard/${themeId}/${topicId}`,
};

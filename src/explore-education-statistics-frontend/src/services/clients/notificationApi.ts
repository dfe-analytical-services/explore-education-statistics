import Client from '@common/services/api/Client';

const notificationApi = new Client({
  baseURL: process.env.NOTIFICATION_API_BASE_URL,
});

export default notificationApi;

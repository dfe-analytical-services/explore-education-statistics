import Client from '@common/services/api/Client';
import axios from 'axios';
import qs from 'qs';

const notificationApi = new Client(
  axios.create({
    baseURL: process.env.NOTIFICATION_API_BASE_URL,
    paramsSerializer: params => qs.stringify(params, { arrayFormat: 'comma' }),
  }),
);

export default notificationApi;

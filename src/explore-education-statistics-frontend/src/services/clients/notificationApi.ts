import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';

const notificationApi = new Client(
  axios.create({
    baseURL: process.env.NOTIFICATION_API_BASE_URL,
    paramsSerializer: commaSeparated,
  }),
);

export default notificationApi;

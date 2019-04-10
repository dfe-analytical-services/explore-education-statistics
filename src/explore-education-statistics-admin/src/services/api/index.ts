import axios from 'axios';
import Client from '../../services/api/Client';
import { commaSeparated } from '../util/paramSerializers';

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL,
};

console.log("CONTENT API : " + process.env.CONTENT_API_BASE_URL);

export const contentApi = new Client(

  axios.create({
    baseURL: `${baseUrl.content}/api/`,
    paramsSerializer: commaSeparated,
  }),
);

export const dataApi = new Client(
  axios.create({
    baseURL: `${baseUrl.data}/api/`,
    paramsSerializer: commaSeparated,
  }),
);

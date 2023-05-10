import axios from 'axios';
import { commaSeparated } from '../util/paramSerializers';
import Client from './Client';

// is the problem here????? 
export const contentApi = new Client(
  axios.create({
    baseURL: process.env.NEXT_PUBLIC_CONTENT_API_BASE_URL,
    paramsSerializer: commaSeparated,
    headers: {
      
    }
  }),
);

export const dataApi = new Client(
  axios.create({
    baseURL: process.env.NEXT_PUBLIC_DATA_API_BASE_URL,
    paramsSerializer: commaSeparated,
  }),
);

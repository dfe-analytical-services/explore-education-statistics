import Client from './Client';

export const contentApi = new Client({
  baseURL: process.env.CONTENT_API_BASE_URL,
});

export const dataApi = new Client({
  baseURL: process.env.DATA_API_BASE_URL,
});

import axios from 'axios';

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL,
};

export const contentApi = axios.create({
  baseURL: `${baseUrl.content}/api/`,
});

export const dataApi = axios.create({ baseURL: `${baseUrl.data}/api/` });

import axios from 'axios';

export default axios.create({
  baseURL: `${process.env.CONTENT_API_BASE_URL}/api/`,
});

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL,
};

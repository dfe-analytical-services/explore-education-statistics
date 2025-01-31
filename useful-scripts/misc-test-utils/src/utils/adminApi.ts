import axios from 'axios';

const { ADMIN_URL, JWT_TOKEN } = process.env;

const adminApi = axios.create({
  baseURL: ADMIN_URL,
  headers: {
    Authorization: `Bearer ${JWT_TOKEN}`,
  },
});
export default adminApi;

import axios from 'axios';

const hostname = window && window.location && window.location.hostname;
let baseURL;

if(hostname === 'educationstatisticstest.z6.web.core.windows.net') {
    baseURL = '//content-explore-education-statistics-test.azurewebsites.net';
  } else {
    //baseURL = '//localhost:5010';
    baseURL = 'https://content-explore-education-statistics-test.azurewebsites.net';
  }

export default axios.create({
  baseURL: `${baseURL}/api/`
});

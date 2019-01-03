import axios from 'axios';

const hostname = window && window.location && window.location.hostname;
let baseURL;

if (hostname === 'educationstatisticstest.z6.web.core.windows.net') {
  baseURL = '//content-explore-education-statistics-test.azurewebsites.net';
} else if (hostname === 'educationstatisticsstage.z6.web.core.windows.net') {
  baseURL = '//content-explore-education-statistics-stage.azurewebsites.net';
} else if (hostname === 'educationstatistics.z6.web.core.windows.net') {
  baseURL = '//content-explore-education-statistics-live.azurewebsites.net';
} else {
  baseURL = '//localhost:5010';
}

export default axios.create({
  baseURL: `${baseURL}/api/`,
});

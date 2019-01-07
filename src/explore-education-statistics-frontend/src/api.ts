import axios from 'axios';

const hostname = window && window.location && window.location.hostname;
let contentBase;
let dataBase;

if (hostname === 'educationstatisticstest.z6.web.core.windows.net') {
  contentBase = '//content-explore-education-statistics-test.azurewebsites.net';
  dataBase = '//data-explore-education-statistics-test.azurewebsites.net';
} else if (hostname === 'educationstatisticsstage.z6.web.core.windows.net') {
  contentBase =
    '//content-explore-education-statistics-stage.azurewebsites.net';
  dataBase = '//data-explore-education-statistics-stage.azurewebsites.net';
} else if (hostname === 'educationstatistics.z6.web.core.windows.net') {
  contentBase = '//content-explore-education-statistics-live.azurewebsites.net';
  dataBase = '//data-explore-education-statistics-live.azurewebsites.net';
} else {
  contentBase = '//localhost:5010';
  dataBase = '//localhost:5000';
}

export default axios.create({
  baseURL: `${contentBase}/api/`,
});

export const baseUrl = {
  content: contentBase,
  data: dataBase,
};

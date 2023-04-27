/* eslint-disable import/no-anonymous-default-export */
import * as yup from 'yup';

import './number';
import FileSchema from './file';

export default {
  ...yup,
  file: () => new FileSchema(),
};

import FileSchema from '@common/validation/yup/file';
import * as yup from 'yup';

import './number';

export default {
  ...yup,
  file: () => new FileSchema(),
};

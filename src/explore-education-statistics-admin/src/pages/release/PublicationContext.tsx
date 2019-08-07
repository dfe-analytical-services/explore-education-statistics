import { BasicPublicationDetails } from '@admin/services/common/types';
import * as React from 'react';

export interface BasicPublicationDetailsContextHolder {
  publication?: BasicPublicationDetails;
}

export default React.createContext<BasicPublicationDetailsContextHolder>({});

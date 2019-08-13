import { BasicPublicationDetails } from '@admin/services/common/types';
import * as React from 'react';

export interface ManageRelease {
  publication: BasicPublicationDetails;
  releaseId: string;
}

export default React.createContext<ManageRelease | null>(null);

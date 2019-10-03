import { BasicPublicationDetails } from '@admin/services/common/types';
import * as React from 'react';

export interface ManageRelease {
  publication: BasicPublicationDetails;
  releaseId: string;
  lastModified: Date;
  invalidate: () => void;
}

export default React.createContext<ManageRelease | null>(null);

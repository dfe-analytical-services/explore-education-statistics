import { BasicPublicationDetails } from '@admin/services/common/types';
import { ReleaseStatus } from '@common/services/publicationService';
import * as React from 'react';

export interface ManageRelease {
  publication: BasicPublicationDetails;
  releaseId: string;
  onChangeReleaseStatus: (status: ReleaseStatus) => void;
}

export default React.createContext<ManageRelease>({} as ManageRelease);

import { BasicPublicationDetails } from '@admin/services/common/types';
import { ReleasePublicationStatus } from '@admin/services/release/types';
import * as React from 'react';

export interface ManageRelease {
  publication: BasicPublicationDetails;
  releaseId: string;
  onChangeReleaseStatus: (status: Partial<ReleasePublicationStatus>) => void;
}

export default React.createContext<ManageRelease>({} as ManageRelease);

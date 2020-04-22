import { BasicPublicationDetails } from '@admin/services/common/types';
import { createContext, useContext } from 'react';

export interface ManageRelease {
  publication: BasicPublicationDetails;
  releaseId: string;
  onChangeReleaseStatus: () => void;
}

const ManageReleaseContext = createContext<ManageRelease | undefined>(
  undefined,
);

export function useManageReleaseContext() {
  const context = useContext(ManageReleaseContext);

  if (context === undefined) {
    throw new Error(
      'useManageReleaseContext must be used within a ManageReleaseContext',
    );
  }
  return context;
}

export default ManageReleaseContext;

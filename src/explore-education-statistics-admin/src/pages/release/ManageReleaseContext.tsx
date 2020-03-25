import { BasicPublicationDetails } from '@admin/services/common/types';
import { useContext, createContext } from 'react';

export interface ManageRelease {
  publication: BasicPublicationDetails;
  releaseId: string;
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

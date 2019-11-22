import { ReleaseStatus } from '@common/services/publicationService';

export interface UpdateReleaseStatusRequest {
  releaseStatus: ReleaseStatus;
  internalReleaseNote: string;
}

export default {};

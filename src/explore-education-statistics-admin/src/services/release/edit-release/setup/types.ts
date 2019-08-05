import { BaseReleaseSetupDetailsRequest } from '@admin/services/release/types';

export interface UpdateReleaseSetupDetailsRequest
  extends BaseReleaseSetupDetailsRequest {
  releaseId: string;
}

export default {};
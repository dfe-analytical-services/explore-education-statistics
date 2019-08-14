import { BaseReleaseSummaryDetailsRequest } from '@admin/services/release/types';

export interface UpdateReleaseSummaryDetailsRequest
  extends BaseReleaseSummaryDetailsRequest {
  releaseId: string;
}

export default {};

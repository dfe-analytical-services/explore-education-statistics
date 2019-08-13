import { BaseReleaseSummaryDetailsRequest } from '@admin/services/release/types';

export interface CreateReleaseRequest extends BaseReleaseSummaryDetailsRequest {
  publicationId: string;
  templateReleaseId?: string;
}

export default {};

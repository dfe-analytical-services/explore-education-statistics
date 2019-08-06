import { BaseReleaseSetupDetailsRequest } from '@admin/services/release/types';

export interface CreateReleaseRequest extends BaseReleaseSetupDetailsRequest {
  publicationId: string;
  templateReleaseId?: string;
}

export default {};

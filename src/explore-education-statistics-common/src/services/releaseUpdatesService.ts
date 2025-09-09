import { contentApi } from '@common/services/api';
import { PaginatedList, PaginationRequestParams } from './types/pagination';

export interface ReleaseUpdate {
  date: string;
  summary: string;
}

const releaseDataGuidanceService = {
  getReleaseUpdates(
    publicationSlug: string,
    releaseSlug: string,
    params?: PaginationRequestParams,
  ): Promise<PaginatedList<ReleaseUpdate>> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/updates`,
      { params },
    );
  },
};

export default releaseDataGuidanceService;

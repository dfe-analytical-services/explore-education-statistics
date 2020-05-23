import {
  ReleasePublicationStatus,
  ReleaseSummaryDetails,
} from '@admin/services/release/types';
import { UpdateReleaseSummaryDetailsRequest } from '@admin/services/release/edit-release/summary/types';
import client from '@admin/services/util/service';
import { Overwrite } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';

const service = {
  async getReleaseSummaryDetails(
    releaseId: string,
  ): Promise<ReleaseSummaryDetails> {
    const summary: Overwrite<
      ReleaseSummaryDetails,
      {
        nextReleaseDate?: {
          day: string;
          month: string;
          year: string;
        };
      }
    > = await client.get(`/releases/${releaseId}/summary`);

    return {
      ...summary,
      nextReleaseDate: summary.nextReleaseDate
        ? {
            day: parseNumber(summary.nextReleaseDate?.day) ?? null,
            month: parseNumber(summary.nextReleaseDate?.month) ?? null,
            year: parseNumber(summary.nextReleaseDate?.year) ?? null,
          }
        : undefined,
    };
  },
  updateReleaseSummaryDetails(
    updateRequest: UpdateReleaseSummaryDetailsRequest,
  ): Promise<void> {
    return client.put(
      `/releases/${updateRequest.releaseId}/summary`,
      updateRequest,
    );
  },
  getReleasePublicationStatus(
    releaseId: string,
  ): Promise<ReleasePublicationStatus> {
    return client.get<ReleasePublicationStatus>(
      `/releases/${releaseId}/publication-status`,
    );
  },
};

export default service;

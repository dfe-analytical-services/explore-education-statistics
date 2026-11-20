import client from '@admin/services/utils/service';
import { UpdateFiltersRequest } from '@admin/pages/release/data/components/ReorderFiltersList';
import { UpdateIndicatorsRequest } from '@admin/pages/release/data/components/ReorderIndicatorsList';

export interface PairedIndicator {
  parentIndicatorId: string;
  childIndicatorId: string;
}

export interface PairedIndicators {
  pairedIndicators: PairedIndicator[];
}

const tableBuilderService = {
  updateFilters(
    releaseId: string,
    subjectId: string,
    data: UpdateFiltersRequest,
  ): Promise<void> {
    return client.patch(
      `data/release/${releaseId}/meta/subject/${subjectId}/filters`,
      data,
    );
  },
  updateIndicators(
    releaseId: string,
    subjectId: string,
    data: UpdateIndicatorsRequest,
  ): Promise<void> {
    return client.patch(
      `data/release/${releaseId}/meta/subject/${subjectId}/indicators`,
      data,
    );
  },
  getPairedIndicators(
    releaseId: string,
    subjectId: string,
  ): Promise<PairedIndicators> {
    return client.get(
      `data/release/${releaseId}/meta/subject/${subjectId}/paired-indicators`,
    );
  },
  updatePairedIndicators(
    releaseId: string,
    subjectId: string,
    data: PairedIndicators,
  ): Promise<PairedIndicators> {
    return client.put(
      `data/release/${releaseId}/meta/subject/${subjectId}/paired-indicators`,
      data,
    );
  },
};

export default tableBuilderService;

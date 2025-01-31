import client from '@admin/services/utils/service';
import { UpdateFiltersRequest } from '@admin/pages/release/data/components/ReorderFiltersList';
import { UpdateIndicatorsRequest } from '@admin/pages/release/data/components/ReorderIndicatorsList';

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
};

export default tableBuilderService;

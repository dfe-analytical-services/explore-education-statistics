import client from '@admin/services/utils/service';
import { UpdateFiltersRequest } from '@admin/pages/release/data/components/ReorderFiltersList';
import { UpdateIndicatorsRequest } from '@admin/pages/release/data/components/ReorderIndicatorsList';

const tableBuilderService = {
  updateFilters(
    releaseVersionId: string,
    subjectId: string,
    data: UpdateFiltersRequest,
  ): Promise<void> {
    return client.patch(
      `data/release/${releaseVersionId}/meta/subject/${subjectId}/filters`,
      data,
    );
  },
  updateIndicators(
    releaseVersionId: string,
    subjectId: string,
    data: UpdateIndicatorsRequest,
  ): Promise<void> {
    return client.patch(
      `data/release/${releaseVersionId}/meta/subject/${subjectId}/indicators`,
      data,
    );
  },
};

export default tableBuilderService;

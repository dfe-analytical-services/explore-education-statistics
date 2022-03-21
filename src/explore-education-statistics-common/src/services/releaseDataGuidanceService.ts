import { contentApi } from '@common/services/api';
import {
  PublicationSummary,
  ReleaseSummary,
} from '@common/services/publicationService';

export interface SubjectDataGuidance {
  id: string;
  filename: string;
  name: string;
  content: string;
  timePeriods: {
    from: string;
    to: string;
  };
  geographicLevels: string[];
  variables: {
    label: string;
    value: string;
  }[];
  footnotes: {
    id: string;
    label: string;
  }[];
}

export interface ReleaseDataGuidanceSummary extends ReleaseSummary {
  publication: PublicationSummary;
  dataGuidance: string;
  subjects: SubjectDataGuidance[];
}

const releaseDataGuidanceService = {
  getLatestReleaseDataGuidance(
    publicationSlug: string,
  ): Promise<ReleaseDataGuidanceSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/latest/data-guidance`,
    );
  },
  getReleaseDataGuidance(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<ReleaseDataGuidanceSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/data-guidance`,
    );
  },
};

export default releaseDataGuidanceService;

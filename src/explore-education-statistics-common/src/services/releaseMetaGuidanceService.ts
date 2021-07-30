import { contentApi } from '@common/services/api';
import { ReleaseSummary } from '@common/services/publicationService';

export interface SubjectMetaGuidance {
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

export interface ReleaseMetaGuidanceSummary extends ReleaseSummary {
  metaGuidance: string;
  subjects: SubjectMetaGuidance[];
}

const releaseMetaGuidanceService = {
  getLatestReleaseMetaGuidance(
    publicationSlug: string,
  ): Promise<ReleaseMetaGuidanceSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/latest/meta-guidance`,
    );
  },
  getReleaseMetaGuidance(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<ReleaseMetaGuidanceSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/meta-guidance`,
    );
  },
};

export default releaseMetaGuidanceService;

import client from '@admin/services/utils/service';

export interface ReleaseMetaGuidance {
  id: string;
  content: string;
  subjects: SubjectMetaGuidance[];
}

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
}

const releaseMetaGuidanceService = {
  getMetaGuidance(releaseId: string): Promise<ReleaseMetaGuidance> {
    return client.get(`/release/${releaseId}/meta-guidance`);
  },
  updateMetaGuidance(
    releaseId: string,
    data: {
      content: string;
      subjects: {
        id: string;
        content: string;
      }[];
    },
  ): Promise<ReleaseMetaGuidance> {
    return client.patch(`/release/${releaseId}/meta-guidance`, data);
  },
};

export default releaseMetaGuidanceService;

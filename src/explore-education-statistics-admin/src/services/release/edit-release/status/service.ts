import {UpdateReleaseStatusRequest} from "@admin/services/release/edit-release/status/types";

export interface ReleaseSummaryService {
  getReleaseStatus: (releaseId: string) => Promise<string>,
  updateReleaseStatus: (releaseId: string, values: UpdateReleaseStatusRequest) => Promise<void>;
}

const service: ReleaseSummaryService = {
  getReleaseStatus: () => Promise.resolve('draft'),
  updateReleaseStatus: () => Promise.resolve(),
};

export default service;

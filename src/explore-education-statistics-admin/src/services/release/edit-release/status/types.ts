import {ReleaseStatus} from "@admin/services/dashboard/types";

export interface UpdateReleaseStatusRequest {
  releaseStatus: ReleaseStatus;
  internalReleaseNote: string;
}

export default {};

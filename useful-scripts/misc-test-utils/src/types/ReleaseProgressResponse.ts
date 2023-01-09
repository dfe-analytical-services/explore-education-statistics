export interface ReleaseProgressResponse {
  releaseId: string;
  dataStage: string;
  contentStage: string;
  filesStage: string;
  publishingStage: string;
  overallStage: string;
  lastUpdated: string;
}

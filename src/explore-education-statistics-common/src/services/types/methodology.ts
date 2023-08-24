export type MethodologyApprovalStatus =
  | 'Draft'
  | 'HigherLevelReview'
  | 'Approved';

export interface MethodologySummary {
  id: string;
  slug: string;
  title: string;
  status: MethodologyApprovalStatus;
}

export interface ExternalMethodology {
  title: string;
  url: string;
}

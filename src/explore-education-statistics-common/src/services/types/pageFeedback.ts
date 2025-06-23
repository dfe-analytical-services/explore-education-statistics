export interface PageFeedbackRequest {
  url: string;
  userAgent: string;
  response: PageFeedbackResponse;
  context?: string;
  issue?: string;
  intent?: string;
}

export interface PageFeedbackViewModel {
  id: string;
  created: Date;
  url: string;
  userAgent: string;
  response: PageFeedbackResponse;
  context: string;
  intent: string;
  issue: string;
  read: boolean;
}

export type PageFeedbackResponse =
  | 'Useful'
  | 'NotUseful'
  | 'ProblemEncountered';

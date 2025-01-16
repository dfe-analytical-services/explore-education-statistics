export interface FeedbackRequest {
  url: string;
  userAgent: string;
  response: FeedbackResponse;
  context?: string;
  issue?: string;
  intent?: string;
}

export interface FeedbackViewModel {
  id: string;
  created: Date;
  url: string;
  userAgent: string;
  response: FeedbackResponse;
  context: string;
  intent: string;
  issue: string;
  read: boolean;
}

export type FeedbackResponse = 'Useful' | 'NotUseful' | 'ProblemEncountered';

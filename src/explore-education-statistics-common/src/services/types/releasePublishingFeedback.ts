export interface ReleasePublishingFeedbackRequest {
  emailToken: string;
  response: ReleasePublishingFeedbackResponse;
  additionalFeedback?: string;
}

export type ReleasePublishingFeedbackResponse =
  | 'ExtremelySatisfied'
  | 'VerySatisfied'
  | 'Satisfied'
  | 'SlightlyDissatisfied'
  | 'NotSatisfiedAtAll';

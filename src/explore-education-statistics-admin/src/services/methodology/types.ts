export interface MethodologyStatus {
  id: string;
  title: string;
  status: string;
  publications: MethodologyStatusPublication[];
}

export interface MethodologyStatusPublication {
  id: string;
  title: string;
}

export interface CreateMethodologyRequest {
  title: string;
  publishScheduled: string;
  contactId: string,
}
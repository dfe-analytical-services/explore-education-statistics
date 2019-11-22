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

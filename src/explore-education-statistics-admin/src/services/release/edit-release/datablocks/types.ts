export interface DependentDataBlock {
  name: string;
  contentSectionHeading?: string;
  infographicFilenames: string[];
}

export interface DeleteDataBlockPlan {
  dependentDataBlocks: DependentDataBlock[];
}

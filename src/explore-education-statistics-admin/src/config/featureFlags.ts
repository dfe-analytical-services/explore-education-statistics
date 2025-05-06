export interface FeatureFlags {
  [key: string]: boolean;
}

export const DEFAULT_FLAGS: FeatureFlags = {
  enableReplacementOfPublicApiDataSets: false,
  // Add more flags as needed
};

export interface FeatureFlags {
  enableReplacementOfPublicApiDataSets: boolean;
  // Add more flags as needed
}

export const DEFAULT_FLAGS: FeatureFlags = {
  enableReplacementOfPublicApiDataSets: false,
  // Add more flags as needed
};

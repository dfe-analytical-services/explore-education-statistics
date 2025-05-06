import React from 'react';
import { useFeatureFlag } from '../contexts/FeatureFlagContext';

export default function useFeatureFlagged(flagName: string) {
  const isEnabled = useFeatureFlag(flagName);

  const FeatureFlagged: React.FC<{ children: React.ReactNode }> = ({
    children,
  }) => {
    if (!isEnabled) return null;
    return <>{children}</>;
  };

  return {
    isEnabled,
    FeatureFlagged,
  };
}

import React from 'react';
import { useFeatureFlag } from '../contexts/FeatureFlagContext';
import { DEFAULT_FLAGS } from '../config/featureFlags';

interface FeatureFlaggedProps {
  flagName: keyof typeof DEFAULT_FLAGS;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export default function FeatureFlagged({
  children,
  flagName,
  fallback = null,
}: FeatureFlaggedProps) {
  const isEnabled = useFeatureFlag(flagName);
  if (!isEnabled) return fallback;
  return <>{children}</>;
}

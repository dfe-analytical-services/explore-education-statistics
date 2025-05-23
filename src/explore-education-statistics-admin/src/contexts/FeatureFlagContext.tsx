import React, {
  createContext,
  useContext,
  ReactNode,
  useCallback,
  useMemo,
} from 'react';
import { DEFAULT_FLAGS, FeatureFlags } from '../config/featureFlags';

interface FeatureFlagContextType {
  isFeatureEnabled: (flagName: keyof typeof DEFAULT_FLAGS) => boolean;
}

const FeatureFlagContext = createContext<FeatureFlagContextType>({
  isFeatureEnabled: () => false,
});

interface FeatureFlagProviderProps {
  children: ReactNode;
  initialFlags?: FeatureFlags;
}

export const FeatureFlagProvider: React.FC<FeatureFlagProviderProps> = ({
  children,
  initialFlags = DEFAULT_FLAGS,
}) => {
  const isFeatureEnabled = useCallback(
    (flagName: keyof typeof DEFAULT_FLAGS): boolean => {
      return initialFlags[flagName] || false;
    },
    [initialFlags],
  );
  const value = useMemo(
    () => ({
      isFeatureEnabled,
    }),
    [isFeatureEnabled],
  );

  return (
    <FeatureFlagContext.Provider value={value}>
      {children}
    </FeatureFlagContext.Provider>
  );
};

export const useFeatureFlag = (
  flagName: keyof typeof DEFAULT_FLAGS,
): boolean => {
  const { isFeatureEnabled } = useContext(FeatureFlagContext);
  return isFeatureEnabled(flagName);
};

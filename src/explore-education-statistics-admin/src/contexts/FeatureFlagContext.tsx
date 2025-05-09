import React, {
  createContext,
  useContext,
  ReactNode,
  useMemo,
  useCallback,
} from 'react';
import { DEFAULT_FLAGS, FeatureFlags } from '../config/featureFlags';

interface FeatureFlagContextType {
  flags: FeatureFlags;
  isFeatureEnabled: (flagName: string) => boolean;
}

const FeatureFlagContext = createContext<FeatureFlagContextType>({
  flags: DEFAULT_FLAGS,
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
    (flagName: string): boolean => {
      return initialFlags[flagName] || false;
    },
    [initialFlags],
  );

  const contextValue = useMemo(
    () => ({
      flags: initialFlags,
      isFeatureEnabled,
    }),
    [initialFlags, isFeatureEnabled],
  );

  return (
    <FeatureFlagContext.Provider value={contextValue}>
      {children}
    </FeatureFlagContext.Provider>
  );
};

export const useFeatureFlag = (flagName: string): boolean => {
  const { isFeatureEnabled } = useContext(FeatureFlagContext);
  return isFeatureEnabled(flagName);
};

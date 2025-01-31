import React, { ComponentType, ReactNode } from 'react';

interface ProviderProps {
  children?: ReactNode;
}

export default function composeProviders(
  ...providers: ComponentType<ProviderProps>[]
): ComponentType<ProviderProps> {
  return function Providers({ children }: ProviderProps) {
    return (
      <>
        {providers.reduceRight(
          (acc, Provider) => (
            <Provider>{acc}</Provider>
          ),
          children,
        )}
      </>
    );
  };
}

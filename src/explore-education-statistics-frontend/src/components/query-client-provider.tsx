'use client';

import React, { ReactNode, useState } from 'react';
import {
  QueryClient,
  QueryClientProvider as BaseQueryClientProvider,
} from '@tanstack/react-query';

export default function QueryClientProvider({
  children,
}: {
  children?: ReactNode;
}) {
  const [queryClient] = useState(() => new QueryClient());

  return (
    <BaseQueryClientProvider client={queryClient}>
      {children}
    </BaseQueryClientProvider>
  );
}

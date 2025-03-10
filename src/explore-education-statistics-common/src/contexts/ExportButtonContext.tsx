import { createContext, useContext } from 'react';

export const ExportButtonContext =
  createContext<React.RefObject<HTMLDivElement> | null>(null);

export const useExportButtonContext = () => {
  const context = useContext(ExportButtonContext);
  if (!context) {
    throw new Error('useExportButtonContext must be used within a RefProvider');
  }
  return context;
};

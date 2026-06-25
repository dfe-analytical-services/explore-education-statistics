import themeService, { ThemeSummary } from '@common/services/themeService';
import { UseQueryOptions } from '@tanstack/react-query';

const themeQueries = {
  list(): UseQueryOptions<ThemeSummary[]> {
    return {
      queryKey: ['listThemes'],
      queryFn: () => themeService.listThemes(),
    };
  },
} as const;

export default themeQueries;

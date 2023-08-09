import { createQueryKeys } from '@lukemorales/query-key-factory';
import themeService from '@admin/services/themeService';

const themeQueries = createQueryKeys('theme', {
  listThemes: {
    queryKey: null,
    queryFn: () => themeService.getThemes(),
  },
});

export default themeQueries;

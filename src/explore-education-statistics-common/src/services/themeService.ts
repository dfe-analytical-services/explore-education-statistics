import { contentApi } from '@common/services/api';
import { MethodologySummary } from '@common/services/types/methodology';

export interface PublicationMethodologySummary {
  id: string;
  title: string;
  methodologies: MethodologySummary[];
}
export interface MethodologyTopic {
  id: string;
  title: string;
  publications: PublicationMethodologySummary[];
}

export interface MethodologyTheme {
  id: string;
  title: string;
  summary: string;
  topics: MethodologyTopic[];
}

export interface ThemeSummary {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

const themeService = {
  getMethodologyThemes(): Promise<MethodologyTheme[]> {
    return contentApi.get(`/methodology-themes`);
  },
  listThemes(): Promise<ThemeSummary[]> {
    return contentApi.get(`/themes`);
  },
};

export default themeService;

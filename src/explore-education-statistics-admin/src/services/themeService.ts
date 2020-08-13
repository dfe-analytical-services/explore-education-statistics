import { Topic } from '@admin/services/topicService';
import client from '@admin/services/utils/service';

export interface Theme {
  id: string;
  title: string;
  summary: string;
  slug: string;
  topics: Topic[];
}

interface SaveThemeRequest {
  title: string;
  summary: string;
}

export type CreateThemeRequest = SaveThemeRequest;
export type UpdateThemeRequest = SaveThemeRequest;

const themeService = {
  getThemes(): Promise<Theme[]> {
    return client.get('/themes');
  },
  getTheme(themeId: string): Promise<Theme> {
    return client.get(`/themes/${themeId}`);
  },
  createTheme(theme: CreateThemeRequest): Promise<Theme> {
    return client.post('/themes', theme);
  },
  updateTheme(themeId: string, theme: UpdateThemeRequest): Promise<Theme> {
    return client.put(`/themes/${themeId}`, theme);
  },
};

export default themeService;

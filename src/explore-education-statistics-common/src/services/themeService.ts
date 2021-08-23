import { contentApi } from '@common/services/api';
import { FileInfo } from '@common/services/types/file';
import { MethodologySummary } from '@common/services/types/methodology';

interface BasePublicationSummary {
  id: string;
  title: string;
  slug: string;
  summary: string;
}

export interface PublicationSummary extends BasePublicationSummary {
  legacyPublicationUrl?: string;
}

export interface PublicationDownloadSummary extends BasePublicationSummary {
  downloadFiles: FileInfo[];
  earliestReleaseTime: string;
  latestReleaseTime: string;
  latestReleaseId: string;
}

export interface PublicationMethodologySummary extends BasePublicationSummary {
  methodologies: MethodologySummary[];
}

export interface Topic<Publication = PublicationSummary> {
  id: string;
  title: string;
  summary: string;
  publications: Publication[];
}

export interface Theme<PublicationNode = PublicationSummary> {
  id: string;
  title: string;
  summary: string;
  topics: Topic<PublicationNode>[];
}

export type DownloadTheme = Theme<PublicationDownloadSummary>;

export type MethodologyTheme = Theme<PublicationMethodologySummary>;

interface ListThemesOptions {
  publicationFilter?: 'LatestData';
}

const themeService = {
  listThemes({ publicationFilter }: ListThemesOptions = {}): Promise<Theme[]> {
    return contentApi.get('/themes', {
      params: { publicationFilter },
    });
  },
  getDownloadThemes(): Promise<DownloadTheme[]> {
    return contentApi.get('/download-themes');
  },
  getMethodologyThemes(): Promise<MethodologyTheme[]> {
    return contentApi.get(`/methodology-themes`);
  },
};

export default themeService;

import { AxiosPromise } from 'axios';
import { contentApi } from './api';

export interface Publication {
  id: string;
  slug: string;
  title: string;
  description: string;
  dataSource: string;
  summary: string;
  nextUpdate: string;
  releases: {
    id: string;
    releaseName: string;
    slug: string;
  }[];
  legacyReleases: {
    id: string;
    description: string;
    url: string;
  }[];
}

export interface Release {
  id: string;
  title: string;
  releaseName: string;
  published: string;
  slug: string;
  summary: string;
  publicationId: string;
  publication: Publication;
  updates: {
    id: string;
    releaseId: string;
    on: string;
    reason: string;
  }[];
  content: {
    order: number;
    heading: string;
    caption: string;
    content: {
      type: string;
      body: string;
      heading?: string;
    }[];
  }[];
  keyStatistics: {
    title: string;
    description: string;
  }[];
}

export const getLatestPublicationRelease = (
  publicationSlug: string,
): AxiosPromise<Release> =>
  contentApi.get(`publication/${publicationSlug}/latest`);

export const getPublicationRelease = (
  releaseId: string,
): AxiosPromise<Release> => contentApi.get(`release/${releaseId}`);

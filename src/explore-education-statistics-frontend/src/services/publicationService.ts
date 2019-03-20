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

export interface DataQuery {
  method: string;
  path: string;
  body: string;
}

export interface Chart {
  type: string;
  attributes: string[];
}

export interface ContentBlock {
  type: string;
  body: string;
  heading?: string;
  dataQuery?: DataQuery;
  charts?: Chart[];
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
    content: ContentBlock[];
  }[];
  keyStatistics: {
    title: string;
    description: string;
  }[];
}

export default {
  getLatestPublicationRelease(publicationSlug: string): Promise<Release> {
    return contentApi.get(`publication/${publicationSlug}/latest`);
  },
  getPublicationRelease(releaseId: string): Promise<Release> {
    return contentApi.get(`release/${releaseId}`);
  },
};

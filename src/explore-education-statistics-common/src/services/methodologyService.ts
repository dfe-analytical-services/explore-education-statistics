import { contentApi } from './api';

export interface Publication {
  id: string;
  title: string;
  slug: string;
  summary: string;
}

export interface Topic {
  id: string;
  slug: string;
  title: string;
  summary: string;
  publications: Publication[];
}

export interface Theme {
  id: string;
  slug: string;
  title: string;
  summary: string;
  topics: Topic[];
}

export default {
  getMethodologies(): Promise<Theme[]> {
    return contentApi.get(`Methodology/tree`);
  },
};

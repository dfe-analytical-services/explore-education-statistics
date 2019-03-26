import SchoolType from 'src/services/types/SchoolType';
import { dataApi } from './api';

export interface PublicationMeta {
  publicationId: string;
  characteristics: {
    [group: string]: {
      name: string;
      label: string;
    }[];
  };
  indicators: {
    [group: string]: {
      name: string;
      label: string;
      unit?: string;
    }[];
  };
}

export type CharacteristicsMeta = PublicationMeta['characteristics'];
export type IndicatorsMeta = PublicationMeta['indicators'];
export type IndicatorsMetaItem = IndicatorsMeta[0][0];

export interface CharacteristicsData {
  publicationId: string;
  releaseId: string;
  releaseDate: string;
  result: {
    timePeriod: number;
    schoolType: SchoolType;
    indicators: {
      [indicator: string]: string;
    };
    characteristic?: {
      label: string;
      name: string;
      description?: string | null;
      name2?: string | null;
    };
  }[];
}

export type DataTableResult = CharacteristicsData['result'][0];

export default {
  getCharacteristicsMeta(publicationUuid: string): Promise<PublicationMeta> {
    return dataApi.get(
      `/tablebuilder/meta/CharacteristicDataNational/${publicationUuid}`,
    );
  },
  getNationalCharacteristicsData(query: {
    publicationId: string;
    characteristics: string[];
    indicators: string[];
    schoolTypes: SchoolType[];
    startYear: number;
    endYear: number;
  }): Promise<CharacteristicsData> {
    return dataApi.post('/tablebuilder/characteristics/national', query);
  },
};

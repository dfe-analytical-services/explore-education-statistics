import SchoolType from 'src/services/types/SchoolType';
import { dataApi } from './api';

export interface PublicationMeta {
  publicationId: string;
  attributes: {
    [group: string]: {
      name: string;
      label: string;
      unit?: string;
    }[];
  };
  characteristics: {
    [group: string]: {
      name: string;
      label: string;
    }[];
  };
}

export type AttributesMeta = PublicationMeta['attributes'];
export type AttributesMetaItem = AttributesMeta[0][0];
export type CharacteristicsMeta = PublicationMeta['characteristics'];

export interface CharacteristicsData {
  publicationId: string;
  releaseId: string;
  releaseDate: string;
  result: {
    year: number;
    schoolType: SchoolType;
    attributes: {
      [attribute: string]: string;
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
    attributes: string[];
    characteristics: string[];
    schoolTypes: SchoolType[];
    startYear: number;
    endYear: number;
  }): Promise<CharacteristicsData> {
    return dataApi.post('/tablebuilder/characteristics/national', query);
  },
};

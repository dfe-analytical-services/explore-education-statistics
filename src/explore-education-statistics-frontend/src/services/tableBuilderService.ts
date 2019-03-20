import { AxiosPromise } from 'axios';
import { dataApi } from './api';

export interface IndicatorsMeta {
  [group: string]: {
    name: string;
    label: string;
    unit?: string;
  }[];
}

export interface CharacteristicsMeta {
  [group: string]: {
    name: string;
    label: string;
  }[];
}

export interface PublicationMeta {
  publicationId: string;
  characteristics: CharacteristicsMeta;
  indicators: IndicatorsMeta;
}

export const getCharacteristicsMeta = (
  publicationUuid: string,
): AxiosPromise<PublicationMeta> =>
  dataApi.get(
    `/tablebuilder/meta/CharacteristicDataNational/${publicationUuid}`,
  );

export enum SchoolType {
  Dummy = 'Dummy',
  Total = 'Total',
  State_Funded_Primary = 'State_Funded_Primary',
  State_Funded_Secondary = 'State_Funded_Secondary',
  Special = 'Special',
}

export interface DataTableResult {
  year: number;
  schoolType: SchoolType;
  indicators: {
    [indicator: string]: string;
  };
}

export interface CharacteristicsData {
  publicationId: string;
  releaseId: string;
  releaseDate: string;
  result: DataTableResult[];
}

export const getNationalCharacteristicsData = (
  publicationId: string,
  characteristics: string[],
  indicators: string[],
  schoolTypes: SchoolType[],
  startYear: number,
  endYear: number,
): AxiosPromise<CharacteristicsData> =>
  dataApi.post('/tablebuilder/characteristics/national', {
    characteristics,
    endYear,
    indicators,
    publicationId,
    schoolTypes,
    startYear,
  });

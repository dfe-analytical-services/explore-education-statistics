import client from '@admin/services/utils/service';
import { Dictionary } from '@common/types';

export interface TargetReplacement {
  id: string;
  label: string;
  target?: string;
  valid: boolean;
}

export type GroupReplacement = {
  label: string;
  valid: boolean;
};

export interface FilterReplacement extends TargetReplacement {
  groups: Dictionary<FilterGroupReplacement>;
}

export interface FilterGroupReplacement extends TargetReplacement {
  filters: FilterItemReplacement[];
}

export type FilterItemReplacement = TargetReplacement;

export interface IndicatorReplacement extends TargetReplacement {
  name: string;
}

export interface IndicatorGroupReplacement {
  id: string;
  label: string;
  valid: boolean;
  indicators: IndicatorReplacement[];
}

export interface LocationAttributeReplacement extends TargetReplacement {
  id: string;
  code: string;
  label: string;
  target?: string;
  valid: boolean;
}

export interface LocationReplacement {
  valid: boolean;
  label: string;
  locationAttributes: LocationAttributeReplacement[];
}

export interface TimePeriodsReplacement {
  valid: boolean;
  start: {
    year: number;
    code: string;
    label: string;
    valid: boolean;
  };
  end: {
    year: number;
    code: string;
    label: string;
    valid: boolean;
  };
}

export type FootnoteFilterReplacement = TargetReplacement;

export interface FootnoteFilterGroupReplacement extends TargetReplacement {
  filterId: string;
  filterLabel: string;
}

export interface FootnoteFilterItemReplacement extends TargetReplacement {
  filterId: string;
  filterLabel: string;
  filterGroupId: string;
  filterGroupLabel: string;
}

export interface FootnoteReplacementPlan {
  id: string;
  content: string;
  valid: boolean;
  filters: FootnoteFilterReplacement[];
  filterGroups: FootnoteFilterGroupReplacement[];
  filterItems: FootnoteFilterItemReplacement[];
  indicatorGroups: Dictionary<IndicatorGroupReplacement>;
}

export interface DataBlockReplacementPlan {
  id: string;
  name: string;
  valid: boolean;
  filters: Dictionary<FilterReplacement>;
  indicatorGroups: Dictionary<IndicatorGroupReplacement>;
  locations: Dictionary<LocationReplacement>;
  timePeriods?: TimePeriodsReplacement;
}

export interface MappingStatus {
  locationsComplete: boolean;
  locationsHaveMajorChange: boolean;
  filtersComplete: boolean;
  filtersHaveMajorChange: boolean;
  indicatorsComplete: boolean;
  indicatorsHaveMajorChange: boolean;
  isMajorVersionUpdate: boolean;
}

export interface ApiDataSetVersionPlan {
  id: string;
  dataSetId: string;
  name: string;
  version: string;
  status: string;
  mappingStatus?: MappingStatus;
  readyToPublish: boolean;
  valid: boolean;
}

type MappingType = 'Unset' | 'ManuallySet' | 'AutoSet';

export interface ReplacementMapping<TSource> {
  type: MappingType;
  source: TSource;
  candidateKey?: string;
}

export type UpdateMappingPayload = {
  sourceKey: string;
  candidateKey?: string;
};

export interface MappingsPlan<TSource> {
  candidates: Dictionary<TSource>;
  mappings: Dictionary<ReplacementMapping<TSource>>;
}

export type MappingWithKey<TSource> = {
  sourceKey: string;
} & ReplacementMapping<TSource>;

export interface IndicatorSource {
  id: string;
  name: string; // csv column name
  label: string;
}

export interface LocationSource {
  id: string;
  code: string;
  name: string;
}

export type IndicatorCandidate = IndicatorSource;

export type SourceItem = IndicatorSource | LocationSource /* | FilterSource */;

export type IndicatorMapping = ReplacementMapping<IndicatorSource>;
export type LocationMapping = ReplacementMapping<LocationSource>;

export type IndicatorMappingWithKey = MappingWithKey<IndicatorSource>;
export type LocationMappingWithKey = MappingWithKey<LocationSource>;

export type IndicatorsMappingsPlan = MappingsPlan<IndicatorSource>;
export type LocationMappingsPlan = MappingsPlan<LocationSource>;

export type PlanMappings = {
  indicators: IndicatorsMappingsPlan;
  locations: LocationMappingsPlan;
};

export interface IndicatorsMapping {
  candidates: Dictionary<IndicatorCandidate>;
  mappings: Dictionary<IndicatorMapping>;
}

export interface DataReplacementPlan {
  originalSubjectId: string;
  replacementSubjectId: string;
  dataBlocks: DataBlockReplacementPlan[];
  footnotes: FootnoteReplacementPlan[];
  apiDataSetVersionPlan?: ApiDataSetVersionPlan;
  valid: boolean;
  mapping: PlanMappings;
}

type PlanMappingIndicatorsUpdateResponse = {
  originalId: string;
  originalLabel: string;
  originalColumnName: string;
  originalGroupId: string;
  originalGroupLabel: string;
  status: MappingType;
  replacementId?: string;
  replacementLabel?: string;
  replacementColumnName?: string;
  replacementGroupId?: string;
  replacementGroupLabel?: string;
}[];

type PlanMappingLocationUpdateResponse = {
  originalId: string;
  originalGeographicLevel: string;
  originalCode: string;
  originalName: string;
  replacementId: string;
  replacementGeographicLevel: string;
  replacementCode: string;
  replacementName: string;
  status: MappingType;
}[];

const dataReplacementService = {
  async getReplacementPlan(
    releaseVersionId: string,
    originalFileId: string,
  ): Promise<DataReplacementPlan> {
    const plan: DataReplacementPlan = await client.get(
      `releases/${releaseVersionId}/data/${originalFileId}/replacement-plan`,
    );

    return plan;
  },
  async updatePlanIndicatorMappings(
    releaseVersionId: string,
    originalDataFileId: string,
    replacementDataFileId: string,
    updates: {
      originalId: string;
      newReplacementId?: string;
    }[],
  ): Promise<DataReplacementPlan['mapping']['indicators']['mappings']> {
    const indicatorsMappings: PlanMappingIndicatorsUpdateResponse =
      await client.patch(
        `releases/${releaseVersionId}/data/replacements/mapping/indicators`,
        {
          originalDataFileId,
          replacementDataFileId,
          updates,
        },
      );

    // restructure from PlanMappingIndicatorsUpdateResponse to PlanMappings['indicators']['mappings']
    const planIndicatorMappings: PlanMappings['indicators']['mappings'] =
      Object.fromEntries(
        indicatorsMappings.map(
          ({
            originalId,
            originalLabel,
            originalColumnName,
            status,
            replacementColumnName,
          }) => [
            originalId,
            {
              source: {
                label: originalLabel,
                name: originalColumnName,
                id: originalId,
              },
              type: status,
              candidateKey: replacementColumnName,
            },
          ],
        ),
      );

    return planIndicatorMappings;
  },

  async updatePlanLocationMappings(
    releaseVersionId: string,
    originalDataSetId: string,
    replacementDataSetId: string,
    updates: {
      originalLocationId: string;
      newReplacementLocationId?: string;
    }[],
  ): Promise<DataReplacementPlan['mapping']['locations']['mappings']> {
    const locationMappings: PlanMappingLocationUpdateResponse =
      await client.patch(
        `releases/${releaseVersionId}/data/replacements/mapping/locations`,
        {
          originalDataSetId,
          replacementDataSetId,
          updates,
        },
      );

    // restructure from PlanMappingLocationUpdateResponse to PlanMappings['locations']['mappings']
    const planLocationMappings: PlanMappings['locations']['mappings'] =
      Object.fromEntries(
        locationMappings.map(
          ({
            originalId,
            originalCode,
            status,
            originalName,
            replacementId,
          }) => [
            originalId,
            {
              source: {
                id: originalId,
                name: originalName,
                code: originalCode,
              },
              type: status,
              candidateKey: replacementId,
            },
          ],
        ),
      );

    return planLocationMappings;
  },
  replaceData(
    releaseVersionId: string,
    originalFileIds: string[],
  ): Promise<void> {
    return client.post(`releases/${releaseVersionId}/data/replacements`, {
      originalFileIds,
    });
  },
};

export default dataReplacementService;

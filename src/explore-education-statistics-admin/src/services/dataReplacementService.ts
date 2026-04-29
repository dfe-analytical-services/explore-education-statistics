import client from '@admin/services/utils/service';
import { Dictionary } from '@common/types';

export interface TargetReplacement {
  id: string;
  label: string;
  target?: string;
  valid: boolean;
}

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

export interface LocationReplacement {
  valid: boolean;
  label: string;
  locationAttributes: {
    code: string;
    label: string;
    target?: string;
    valid: boolean;
  }[];
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

interface Mapping<TSource> {
  type: MappingType;
  source: TSource;
  candidateKey?: string;
}

export type UpdateMappingPayload = {
  sourceKey: string;
  candidateKey?: string;
};
export type UpdateMappingPayloadMultiple = UpdateMappingPayload[];

interface MappingsPlan<TSource> {
  candidates: Dictionary<TSource>;
  mappings: Dictionary<Mapping<TSource>>;
}

export type MappingWithKey<TSource> = { sourceKey: string } & Mapping<TSource>;

interface IndicatorSource {
  label: string;
}

export type IndicatorCandidate = IndicatorSource;

export type SourceItem = IndicatorSource;

export type IndicatorMapping = Mapping<IndicatorSource>;
export type IndicatorMappingWithKey = MappingWithKey<IndicatorSource>;

export type IndicatorsMappingsPlan = MappingsPlan<IndicatorSource>;

export type PlanMappings = {
  indicators: IndicatorsMappingsPlan;
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
  apiDataSetVersionPlan: ApiDataSetVersionPlan;
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
    originalDataSetId: string,
    replacementDataSetId: string,
    updates: {
      originalColumnName: string;
      newReplacementColumnName?: string;
    }[],
  ): Promise<DataReplacementPlan['mapping']['indicators']['mappings']> {
    const indicatorsMappings: PlanMappingIndicatorsUpdateResponse =
      await client.patch(
        `releases/${releaseVersionId}/data/replacements/mapping/indicators`,
        {
          originalDataSetId,
          replacementDataSetId,
          updates,
        },
      );

    // restructure from PlanMappingIndicatorsUpdateResponse to PlanMappings['indicators']['mappings']
    const planIndicatorMappings: PlanMappings['indicators']['mappings'] =
      Object.fromEntries(
        indicatorsMappings.map(
          ({
            originalLabel,
            originalColumnName,
            status,
            replacementColumnName,
          }) => [
            originalColumnName,
            {
              source: { label: originalLabel },
              type: status,
              candidateKey: replacementColumnName,
            },
          ],
        ),
      );

    return planIndicatorMappings;
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

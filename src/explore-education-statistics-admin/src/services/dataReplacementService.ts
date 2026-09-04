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
  items: FilterItemReplacement[];
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

export type MappingType =
  'Unset' | 'ManuallySet' | 'AutoSet' | 'ParentNotMapped';

export interface Replacement<TSource> {
  type: MappingType;
  source: TSource;
}

export interface ReplacementMapping<TSource> extends Replacement<TSource> {
  candidateKey?: string;
}

export type UpdateMappingPayload = {
  sourceKey: string;
  candidateKey?: string;
};

export interface FilterMappingSource {
  id: string;
  label: string;
}

export type Candidates<TSource = CandidateSource> = Dictionary<TSource>;
export type Mappings<
  TSource = CandidateSource,
  TMapping = ReplacementMapping<TSource>,
> = Dictionary<TMapping>;

export interface MappingsPlan<
  TSource = CandidateSource,
  TMapping = ReplacementMapping<TSource>,
> {
  candidates: Candidates<TSource>;
  mappings: Mappings<TSource, TMapping>;
}

export type FilterMappingFilterItems<TSource> = MappingsPlan<
  TSource,
  ReplacementMapping<FilterMappingSource>
>;

export interface FilterGroupMappings<
  TSource,
> extends ReplacementMapping<TSource> {
  filterItems: FilterMappingFilterItems<TSource>;
}

export type FilterMappingFilterGroups = MappingsPlan<
  FilterMappingSource,
  FilterGroupMappings<FilterMappingSource>
>;

export interface FilterMappingReplacementMapping<
  TSource,
> extends ReplacementMapping<TSource> {
  filterGroups: FilterMappingFilterGroups;
}

export interface CandidateSource {
  id: string;
  name: string;
}

export interface FilterSource extends CandidateSource {
  label: string;
}

export interface IndicatorSource extends CandidateSource {
  label: string;
}

export interface LocationSource extends CandidateSource {
  code: string;
}

export type IndicatorCandidate = IndicatorSource;

export type IndicatorMapping = ReplacementMapping<IndicatorSource>;
export type LocationMapping = ReplacementMapping<LocationSource>;

export type IndicatorsMappingsPlan = MappingsPlan<IndicatorSource>;
export type LocationMappingsPlan = MappingsPlan<LocationSource>;
export type FilterMappingPlan = MappingsPlan<
  FilterSource,
  FilterMappingReplacementMapping<FilterSource>
>;

export type PlanMappings = {
  indicators: IndicatorsMappingsPlan;
  locations: LocationMappingsPlan;
  filters: FilterMappingPlan;
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
  mapping: PlanMappings;
  // a replacement is invalid if a data blocks exist and an additional filter has been added to the replacement, as
  // data blocks will be missing a filter item
  hasDataBlockAndReplacementHasAdditionalFilter: boolean;
  valid: boolean;
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

type FilterMappingResponse = {
  originalId: string;
  originalLabel: string;
  originalColumnName: string;

  replacementId?: string;
  replacementLabel?: string;
  replacementColumnName?: string;

  status: MappingType;
};
type PlanMappingFilterUpdateResponse = {
  filters: FilterMappingResponse[];
  filterGroups: FilterMappingResponse[];
  filterItems: FilterMappingResponse[];
};

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
    originalDataFileId: string,
    replacementDataFileId: string,
    updates: {
      originalId: string;
      newReplacementId?: string;
    }[],
  ): Promise<DataReplacementPlan['mapping']['locations']['mappings']> {
    const locationMappings: PlanMappingLocationUpdateResponse =
      await client.patch(
        `releases/${releaseVersionId}/data/replacements/mapping/locations`,
        {
          originalDataFileId,
          replacementDataFileId,
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
  async updatePlanFilterMappings(
    releaseVersionId: string,
    originalDataFileId: string,
    replacementDataFileId: string,
    filterUpdates: { originalId: string; newReplacementId?: string }[],
    filterGroupUpdates: { originalId: string; newReplacementId?: string }[],
    filterItemUpdates: { originalId: string; newReplacementId?: string }[],
  ): Promise<PlanMappingFilterUpdateResponse> {
    const filterMappings: PlanMappingFilterUpdateResponse = await client.patch(
      `releases/${releaseVersionId}/data/replacements/mapping/filters`,
      {
        originalDataFileId,
        replacementDataFileId,
        filterUpdates,
        filterGroupUpdates,
        filterItemUpdates,
      },
    );

    // the service can only return what it knows, it's up to the caller to integrate into its own data
    return filterMappings;
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

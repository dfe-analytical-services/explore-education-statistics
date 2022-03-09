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
  fixable: boolean;
  filters: Dictionary<FilterReplacement>;
  indicatorGroups: Dictionary<IndicatorGroupReplacement>;
  locations: Dictionary<LocationReplacement>;
  timePeriods?: TimePeriodsReplacement;
}

export interface DataReplacementPlan {
  originalSubjectId: string;
  replacementSubjectId: string;
  dataBlocks: DataBlockReplacementPlan[];
  footnotes: FootnoteReplacementPlan[];
  valid: boolean;
}

const dataReplacementService = {
  getReplacementPlan(
    releaseId: string,
    fileId: string,
    replacementFileId: string,
  ): Promise<DataReplacementPlan> {
    return client.get(
      `releases/${releaseId}/data/${fileId}/replacement-plan/${replacementFileId}`,
    );
  },
  replaceData(
    releaseId: string,
    fileId: string,
    replacementFileId: string,
  ): Promise<void> {
    return client.post(
      `releases/${releaseId}/data/${fileId}/replacement/${replacementFileId}`,
    );
  },
};

export default dataReplacementService;

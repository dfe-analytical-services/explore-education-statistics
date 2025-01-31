import { ValueLabelPair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';

export interface TimePeriodCoverageGroup {
  category: {
    label: string;
  };
  timeIdentifiers: {
    identifier: ValueLabelPair;
  }[];
}

const metaService = {
  getTimePeriodCoverageGroups(): Promise<TimePeriodCoverageGroup[]> {
    return client.get<TimePeriodCoverageGroup[]>('/meta/timeidentifiers');
  },
};

export default metaService;

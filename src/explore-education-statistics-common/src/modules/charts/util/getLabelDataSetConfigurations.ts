import {
  DataSet,
  DataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import { generateDeprecatedDataSetKey } from '@common/modules/charts/util/deprecatedDataSetKey';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { DeprecatedChartLabels } from '@common/services/types/blocks';
import omit from 'lodash/omit';

/**
 * Combine {@param labels} to matching {@param dataSets}.
 * In the future, we won't need this function as {@param labels}
 * will be removed from chart data structures completely.
 */
export default function getLabelDataSetConfigurations(
  labels: DeprecatedChartLabels,
  dataSets: DataSet[],
): DataSetConfiguration[] {
  return dataSets
    .map<DataSetConfiguration | undefined>(dataSet => {
      let key = generateDataSetKey(dataSet);

      let configuration = labels[key];

      // Re-attempt to find the data set's label config
      // using the deprecated key as we have to support
      // both for the time being.
      if (!configuration) {
        key = generateDeprecatedDataSetKey(dataSet);
        configuration = labels[key];
      }

      if (!configuration) {
        return undefined;
      }

      return {
        ...dataSet,
        config: omit(configuration ?? {}, ['value']),
      };
    })
    .filter(
      dataSet => typeof dataSet !== 'undefined',
    ) as DataSetConfiguration[];
}

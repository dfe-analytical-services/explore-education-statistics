import { ChartBuilderForm } from '@admin/pages/release/datablocks/components/ChartBuilder';
import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/ChartBuilderSaveActions';
import ChartDataConfiguration from '@admin/pages/release/datablocks/components/ChartDataConfiguration';
import styles from '@admin/pages/release/datablocks/components/ChartDataSetsConfiguration.module.scss';
import ChartDataSetsAddForm from '@admin/pages/release/datablocks/components/ChartDataSetsAddForm';
import { FormState } from '@admin/pages/release/datablocks/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { ChartDefinition } from '@common/modules/charts/types/chart';
import {
  DataSet,
  DataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import { colours, symbols } from '@common/modules/charts/util/chartUtils';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import generateDefaultDataSetLabel from '@common/modules/charts/util/generateDefaultDataSetLabel';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import difference from 'lodash/difference';
import React, { ReactNode, useEffect, useState } from 'react';

interface Props {
  buttons?: ReactNode;
  canSaveChart: boolean;
  dataSets?: DataSetConfiguration[];
  definition: ChartDefinition;
  forms: Dictionary<ChartBuilderForm>;
  isSaving?: boolean;
  meta: FullTableMeta;
  onDataAdded?: (data: DataSetConfiguration) => void;
  onDataRemoved?: (data: DataSetConfiguration, index: number) => void;
  onDataChanged?: (data: DataSetConfiguration[]) => void;
  onFormStateChange: (
    state: {
      form: 'data';
    } & FormState,
  ) => void;
  onSubmit: (data: DataSetConfiguration[]) => void;
}

const formId = 'chartDataSetsConfigurationForm';

const ChartDataSetsConfiguration = ({
  buttons,
  canSaveChart,
  isSaving,
  forms,
  meta,
  dataSets = [],
  definition,
  onDataRemoved,
  onDataAdded,
  onDataChanged,
  onFormStateChange,
  onSubmit,
}: Props) => {
  const { capabilities } = definition;

  const [dataSetConfigs, setDataSetConfigs] = useState<DataSetConfiguration[]>([
    ...dataSets,
  ]);

  const [submitCount, setSubmitCount] = useState(0);

  useEffect(() => {
    onFormStateChange({
      form: 'data',
      isValid: true,
      submitCount,
    });
  }, [onFormStateChange, submitCount]);

  const removeSelected = (selected: DataSetConfiguration, index: number) => {
    const newDataSets = [...dataSetConfigs];
    const [removed] = newDataSets.splice(index, 1);

    setDataSetConfigs(newDataSets);

    if (onDataRemoved) {
      onDataRemoved(removed, index);
    }
  };

  return (
    <>
      <ChartDataSetsAddForm
        meta={meta}
        onSubmit={values => {
          const { indicator } = values;
          const filters = Object.values(values.filters);

          // Convert empty strings from form values to undefined
          const timePeriod: DataSet['timePeriod'] = values.timePeriod
            ? values.timePeriod
            : undefined;

          const location: DataSet['location'] = values.location
            ? LocationFilter.parseCompositeId(values.location)
            : undefined;

          if (
            dataSetConfigs.find(dataSet => {
              return (
                dataSet.indicator === indicator &&
                difference(dataSet.filters, filters).length === 0 &&
                dataSet.location?.level === location?.level &&
                dataSet.location?.value === location?.value &&
                dataSet.timePeriod === timePeriod
              );
            })
          ) {
            throw new Error(
              'The selected options have already been added to the chart',
            );
          }

          const dataSet: DataSet = {
            filters,
            indicator,
            location,
            timePeriod,
          };

          const expandedDataSet = expandDataSet(dataSet, meta);

          const label = generateDefaultDataSetLabel(expandedDataSet);

          const newDataSetConfig: DataSetConfiguration = {
            ...dataSet,
            config: {
              label,
              colour: colours[dataSetConfigs.length % colours.length],
              symbol: symbols[dataSetConfigs.length % symbols.length],
            },
          };

          setDataSetConfigs([...dataSetConfigs, newDataSetConfig]);

          if (onDataAdded) {
            onDataAdded(newDataSetConfig);
          }
        }}
      />

      {dataSetConfigs.length > 0 && (
        <>
          <hr />

          <ul className={styles.dataSets}>
            {dataSetConfigs.map((dataSet, index) => {
              const expandedDataSet = expandDataSet(dataSet, meta);
              const label = generateDefaultDataSetLabel(expandedDataSet);

              return (
                <li key={generateDataSetKey(dataSet)}>
                  <div className={styles.dataSetRow}>
                    <span>{label}</span>

                    <div>
                      <Button
                        onClick={() => removeSelected(dataSet, index)}
                        className="govuk-!-margin-bottom-0 govuk-button--secondary"
                      >
                        Remove
                      </Button>
                    </div>
                  </div>

                  <Details
                    summary="Change styling"
                    className="govuk-!-margin-bottom-3"
                  >
                    <ChartDataConfiguration
                      capabilities={capabilities}
                      dataSet={dataSet}
                      id={`${formId}-chartDataConfiguration-${index}`}
                      onConfigurationChange={updatedDataSetConfig => {
                        const nextDataSetConfigs = [...dataSetConfigs];

                        nextDataSetConfigs[index] = {
                          ...nextDataSetConfigs[index],
                          config: updatedDataSetConfig,
                        };

                        setDataSetConfigs(nextDataSetConfigs);

                        if (onDataChanged) {
                          onDataChanged(nextDataSetConfigs);
                        }
                      }}
                    />
                  </Details>
                </li>
              );
            })}
          </ul>

          <ChartBuilderSaveActions
            disabled={isSaving}
            formId={formId}
            forms={forms}
            showSubmitError={submitCount > 0 && !canSaveChart}
            onClick={() => {
              setSubmitCount(submitCount + 1);

              if (canSaveChart) {
                onSubmit(dataSetConfigs);
              }
            }}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </>
      )}
    </>
  );
};

export default ChartDataSetsConfiguration;

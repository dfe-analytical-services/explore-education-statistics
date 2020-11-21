import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/ChartBuilderSaveActions';
import ChartDataSetsAddForm from '@admin/pages/release/datablocks/components/ChartDataSetsAddForm';
import { ChartBuilderForms } from '@admin/pages/release/datablocks/components/types/chartBuilderForms';
import generateDataSetLabel from '@admin/pages/release/datablocks/utils/generateDataSetLabel';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { DataSet } from '@common/modules/charts/types/dataSet';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import difference from 'lodash/difference';
import React, { ReactNode } from 'react';

const formId = 'chartDataSetsConfigurationForm';

interface Props {
  buttons?: ReactNode;
  dataSets?: DataSet[];
  forms: ChartBuilderForms;
  isSaving?: boolean;
  meta: FullTableMeta;
  onDataAdded?: (data: DataSet) => void;
  onDataRemoved?: (data: DataSet, index: number) => void;
  onSubmit: () => void;
}

const ChartDataSetsConfiguration = ({
  buttons,
  isSaving,
  forms,
  meta,
  dataSets = [],
  onDataRemoved,
  onDataAdded,
  onSubmit,
}: Props) => {
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
            dataSets.find(dataSet => {
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

          if (onDataAdded) {
            onDataAdded({
              filters,
              indicator,
              location,
              timePeriod,
            });
          }
        }}
      />

      {dataSets?.length > 0 && (
        <>
          <table>
            <thead>
              <tr>
                <th>Data set</th>
                <th>
                  <VisuallyHidden>Actions</VisuallyHidden>
                </th>
              </tr>
            </thead>
            <tbody>
              {dataSets.map((dataSet, index) => {
                const expandedDataSet = expandDataSet(dataSet, meta);
                const label = generateDataSetLabel(expandedDataSet);

                return (
                  <tr key={generateDataSetKey(dataSet)}>
                    <td>{label}</td>
                    <td className="dfe-align--right">
                      <ButtonText
                        className="govuk-!-margin-bottom-0"
                        onClick={() => {
                          if (onDataRemoved) {
                            onDataRemoved(dataSet, index);
                          }
                        }}
                      >
                        Remove
                      </ButtonText>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>

          <ChartBuilderSaveActions
            disabled={isSaving}
            formId={formId}
            forms={forms}
            onClick={() => {
              onSubmit();
            }}
            showSubmitError={forms.data.submitCount > 0}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </>
      )}
    </>
  );
};

export default ChartDataSetsConfiguration;

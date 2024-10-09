import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import Modal from '@common/components/Modal';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import React, { useCallback, useState } from 'react';
import { useFieldArray, UseFormReturn } from 'react-hook-form';
import type { ChartBoundaryLevelsFormValues } from './ChartBoundaryLevelsConfiguration';
import generateDataSetLabel from './utils/generateDataSetLabel';

interface Props {
  control: UseFormReturn<ChartBoundaryLevelsFormValues>['control'];
  meta: FullTableMeta;
}

export default function ChartBoundaryLevelsDataSetConfiguration({
  control,
  meta,
}: Props) {
  const [updateIndex, setUpdateIndex] = useState<number>();
  const [selectValue, setSelectValue] = useState<number>();
  const { fields: dataSetConfigs, update } = useFieldArray<
    ChartBoundaryLevelsFormValues,
    'dataSetConfigs'
  >({
    control,
    name: 'dataSetConfigs',
  });

  const openFormModal = useCallback(
    (i: number) => {
      setUpdateIndex(i);
      setSelectValue(dataSetConfigs[i].boundaryLevel);
    },
    [setUpdateIndex, dataSetConfigs],
  );

  const closeFormModal = useCallback(() => {
    setUpdateIndex(undefined);
    setSelectValue(undefined);
  }, [setUpdateIndex]);

  return (
    <>
      <h4>Set boundary levels per data set</h4>
      {/* {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>} */}

      {!!dataSetConfigs && dataSetConfigs.length > 1 && (
        <table data-testid="chart-boundary-level-selections">
          <thead>
            <tr>
              <th>Data set</th>
              <th>Boundary</th>
              <th className="govuk-!-text-align-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {dataSetConfigs.map((dataSetConfig, index) => {
              const expandedDataSet = expandDataSet(
                dataSetConfig.dataSet,
                meta,
              );
              const label = generateDataSetLabel(expandedDataSet);
              const key = generateDataSetKey(dataSetConfig.dataSet);

              const boundaryLabel = dataSetConfig.boundaryLevel
                ? meta.boundaryLevels.find(
                    ({ id }) =>
                      String(id) === String(dataSetConfig.boundaryLevel),
                  )?.label
                : 'Default';

              return (
                <tr key={key}>
                  <td>{label}</td>
                  <td>{boundaryLabel}</td>
                  <td className="govuk-!-text-align-right">
                    <ButtonText
                      onClick={() => {
                        openFormModal(index);
                      }}
                    >
                      Edit
                    </ButtonText>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}

      {updateIndex !== undefined && (
        <Modal open title="Edit boundary" onExit={closeFormModal}>
          <FormFieldSelect
            label="Boundary level"
            hint="Select a version of geographical data to use for this dataset"
            name="dataSetBoundaryLevel"
            order={[]}
            options={[
              {
                label: 'Default',
                value: '',
              },
              ...meta.boundaryLevels.map(({ id, label }) => ({
                value: id,
                label,
              })),
            ]}
            onChange={({ target }) => {
              // works but dodgy type?
              const { value } = target;
              setSelectValue(
                Number.isNaN(Number(value)) ? undefined : Number(value),
              );
            }}
          />
          <ButtonGroup>
            <Button
              onClick={() => {
                const { dataSet, dataGrouping } = dataSetConfigs[updateIndex];
                update(updateIndex, {
                  dataSet,
                  dataGrouping,
                  boundaryLevel: selectValue,
                });
                closeFormModal();
              }}
            >
              Done
            </Button>
            <Button onClick={closeFormModal} variant="secondary">
              Cancel
            </Button>
          </ButtonGroup>
        </Modal>
      )}
    </>
  );
}

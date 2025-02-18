import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import Effect from '@common/components/Effect';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormProvider from '@common/components/form/FormProvider';
import { SelectOption } from '@common/components/form/FormSelect';
import { MapDataSetConfig } from '@common/modules/charts/types/chart';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import Yup from '@common/validation/yup';
import React, { ReactNode, useCallback, useMemo } from 'react';
import { ObjectSchema } from 'yup';

const formId = 'chartBoundaryLevelsConfigurationForm';

export interface ChartBoundaryLevelsFormValues {
  boundaryLevel?: string;
  dataSetConfigs: {
    boundaryLevel?: string;
  }[];
}

interface Props {
  buttons?: ReactNode;
  initialValues: ChartBoundaryLevelsFormValues;
  dataSetConfigs: MapDataSetConfig[];
  meta: FullTableMeta;
  onChange: (values: Partial<ChartBoundaryLevelsFormValues>) => void;
  onSubmit: (values: ChartBoundaryLevelsFormValues) => void;
}

export default function ChartBoundaryLevelsForm({
  buttons,
  dataSetConfigs,
  initialValues,
  meta,
  onChange,
  onSubmit,
}: Props) {
  const { updateForm, submitForms } = useChartBuilderFormsContext();

  const boundaryLevelOptions = useMemo<SelectOption<string>[]>(() => {
    return meta.boundaryLevels.map(level => {
      return {
        label: level.label,
        value: level.id.toString(),
      };
    });
  }, [meta.boundaryLevels]);

  const dataSetRows = useMemo(() => {
    return dataSetConfigs.map(dataSetConfig => {
      const expandedDataSet = expandDataSet(dataSetConfig.dataSet, meta);
      const label = generateDataSetLabel(expandedDataSet);
      const key = generateDataSetKey(dataSetConfig.dataSet);

      return {
        key,
        label,
      };
    });
  }, [dataSetConfigs, meta]);

  const validationSchema = useMemo<
    ObjectSchema<ChartBoundaryLevelsFormValues>
  >(() => {
    return Yup.object({
      boundaryLevel: Yup.string().required('Choose a boundary level'),
      dataSetConfigs: Yup.array()
        .of(
          Yup.object({
            boundaryLevel: Yup.string().optional(),
          }),
        )
        .required(),
    });
  }, []);

  const handleSubmit = useCallback(
    async (values: ChartBoundaryLevelsFormValues) => {
      onSubmit(values);
      await submitForms();
    },
    [onSubmit, submitForms],
  );

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      {({ formState, reset }) => {
        return (
          <Form<ChartBoundaryLevelsFormValues>
            id={formId}
            onChange={onChange}
            onSubmit={handleSubmit}
          >
            <Effect
              value={dataSetRows}
              onChange={() => {
                reset();
              }}
            />
            <Effect
              value={{
                formKey: 'boundaryLevels',
                isValid: formState.isValid,
                submitCount: formState.submitCount,
              }}
              onChange={updateForm}
              onMount={updateForm}
            />
            <FormFieldSelect<ChartBoundaryLevelsFormValues>
              label="Default boundary level"
              hint="Select a version of geographical data to use across any data sets that don't have a specific one set"
              name="boundaryLevel"
              order={[]}
              options={[
                {
                  label: 'Please select',
                  value: '',
                },
                ...boundaryLevelOptions,
              ]}
            />
            {dataSetRows.length > 1 && (
              <>
                <h4>Set boundary levels per data set</h4>
                <table data-testid="data-set-boundary-levels">
                  <thead>
                    <tr>
                      <th>Data set</th>
                      <th>Boundary level</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dataSetRows.map(({ key, label }, index) => {
                      return (
                        <tr key={key}>
                          <td>{label}</td>
                          <td>
                            <FormFieldSelect
                              label={`Boundary level for data set: ${label}`}
                              hideLabel
                              name={`dataSetConfigs[${index}].boundaryLevel`}
                              order={[]}
                              options={[
                                {
                                  label: 'Use default',
                                  value: '',
                                },
                                ...boundaryLevelOptions,
                              ]}
                            />
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </>
            )}

            <ChartBuilderSaveActions
              formId={formId}
              formKey="boundaryLevels"
              disabled={formState.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        );
      }}
    </FormProvider>
  );
}

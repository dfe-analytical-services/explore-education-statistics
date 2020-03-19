import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import { ChartOptions } from '@admin/pages/release/edit-release/manage-datablocks/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldSelect,
  FormFieldTextInput,
  FormGroup,
  Formik,
} from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import {
  ChartDefinition,
  ChartMetaData,
} from '@common/modules/charts/types/chart';
import { DataBlockResponse } from '@common/services/dataBlockService';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import React, { useCallback } from 'react';
import { Schema } from 'yup';
import InfographicChartForm from './InfographicChartForm';

type ChartOptionsChangeValue = ChartOptions & { isValid: boolean };

interface Props {
  selectedChartType: ChartDefinition;
  chartOptions: ChartOptions;
  data: DataBlockResponse;
  meta: ChartMetaData;
  onBoundaryLevelChange?: (boundaryLevel: string) => void;
  onChange: (chartOptions: ChartOptionsChangeValue) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

const formId = 'chartConfigurationForm';

const ChartConfiguration = ({
  chartOptions,
  selectedChartType,
  meta,
  data,
  onBoundaryLevelChange,
  onChange,
  onSubmit,
}: Props) => {
  const normalizeValues = (values: ChartOptions): ChartOptions => {
    return {
      ...values,
      width: parseNumber(values.width),
    };
  };

  const handleChange = useCallback(
    ({ isValid, ...values }: ChartOptionsChangeValue) => {
      onChange({
        ...normalizeValues(values),
        isValid,
      });
    },
    [onChange],
  );

  return (
    <>
      {selectedChartType.type === 'infographic' && (
        <>
          <InfographicChartForm
            releaseId={data.releaseId}
            fileId={chartOptions.fileId || ''}
            onSubmit={fileId => {
              onChange({
                ...chartOptions,
                fileId,
                isValid: true,
              });
            }}
          />
          <hr />
        </>
      )}

      <Formik<ChartOptions>
        initialValues={chartOptions}
        onSubmit={values => {
          onSubmit(normalizeValues(values));
        }}
        validationSchema={Yup.object<ChartOptions>({
          height: Yup.number()
            .required('Enter chart height')
            .positive('Chart height must be positive'),
          width: Yup.number().positive('Chart width must be positive'),
          legend: Yup.string().oneOf(
            ['bottom', 'top', 'none'],
            'Select a valid legend position',
          ) as Schema<ChartOptions['legend']>,
          stacked: Yup.boolean(),
        })}
        render={form => (
          <Form id={formId}>
            <Effect
              value={{
                ...form.values,
                isValid: form.isValid,
              }}
              onChange={handleChange}
            />

            <FormGroup>
              <FormFieldTextInput<ChartOptions>
                id={`${formId}-title`}
                name="title"
                label="Chart title"
                percentageWidth="three-quarters"
              />
            </FormGroup>

            {selectedChartType.capabilities.stackable && (
              <FormGroup>
                <FormFieldCheckbox<ChartOptions>
                  id={`${formId}-stacked`}
                  name="stacked"
                  label="Stacked bars"
                  className={styles['margin-top-30']}
                />
              </FormGroup>
            )}

            {selectedChartType.capabilities.hasLegend && (
              <FormGroup>
                <FormFieldSelect<ChartOptions>
                  id={`${formId}-position`}
                  name="legend"
                  label="Legend position"
                  options={[
                    { label: 'Top', value: 'top' },
                    { label: 'Bottom', value: 'bottom' },
                    { label: 'None', value: 'none' },
                  ]}
                  order={[]}
                />
              </FormGroup>
            )}

            {selectedChartType.capabilities.canSize && (
              <>
                <FormGroup>
                  <FormFieldTextInput<ChartOptions>
                    type="number"
                    id={`${formId}-height`}
                    name="height"
                    label="Chart height (px)"
                    width={5}
                  />
                </FormGroup>
                <FormGroup>
                  <FormFieldTextInput<ChartOptions>
                    type="number"
                    id={`${formId}-width`}
                    name="width"
                    label="Chart width (px)"
                    hint="Leave blank to set as full width"
                    width={5}
                  />
                </FormGroup>
              </>
            )}

            {selectedChartType.type === 'map' && meta.boundaryLevels && (
              <>
                {meta.boundaryLevels.length === 1 && (
                  <div>
                    Using <em>{meta.boundaryLevels[0].label}</em>
                  </div>
                )}
                {meta.boundaryLevels.length > 1 && (
                  <FormGroup>
                    <FormFieldSelect<ChartOptions>
                      id={`${formId}-geographicId`}
                      label="Select a version of geographical data to use"
                      name="geographicId"
                      order={[]}
                      options={[
                        { label: 'Latest', value: '' },
                        ...meta.boundaryLevels.map(({ id, label }) => ({
                          value: id,
                          label,
                        })),
                      ]}
                      onChange={e => {
                        if (onBoundaryLevelChange) {
                          onBoundaryLevelChange(e.target.value);
                        }
                      }}
                    />
                  </FormGroup>
                )}
              </>
            )}

            <Button type="submit">Save chart options</Button>
          </Form>
        )}
      />
    </>
  );
};

export default ChartConfiguration;

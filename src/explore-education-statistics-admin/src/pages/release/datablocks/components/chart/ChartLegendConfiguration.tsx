import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import Effect from '@common/components/Effect';
import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartSymbol,
  LineStyle,
} from '@common/modules/charts/types/chart';
import { DataSet, DataSetCategory } from '@common/modules/charts/types/dataSet';
import {
  LegendConfiguration,
  LegendItem,
  LegendItemConfiguration,
  LegendPosition,
  LegendInlinePosition,
} from '@common/modules/charts/types/legend';
import { legendPositions } from '@common/modules/charts/util/chartUtils';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import Yup from '@common/validation/yup';
import toPath from 'lodash/toPath';
import upperFirst from 'lodash/upperFirst';
import React, { ReactNode, useCallback, useMemo, useRef } from 'react';
import { ObjectSchema } from 'yup';
import ChartLegendItems from './ChartLegendItems';

const positionOptions: SelectOption[] = legendPositions.map(position => ({
  label: upperFirst(position),
  value: position,
}));

function getLegendItemNumber(path: string): number {
  const [, index] = toPath(path);
  return +index + 1;
}

export type ChartLegendFormValues = LegendConfiguration;

const formId = 'chartLegendConfigurationForm';

interface Props {
  axisMajor: AxisConfiguration;
  buttons?: ReactNode;
  data: TableDataResult[];
  definition: ChartDefinition;
  legend: LegendConfiguration;
  meta: FullTableMeta;
  showDataLabels?: boolean;
  onChange: (legend: LegendConfiguration) => void;
  onSubmit: (legend: LegendConfiguration) => void;
}

const ChartLegendConfiguration = ({
  axisMajor,
  buttons,
  data,
  definition,
  legend,
  meta,
  showDataLabels,
  onChange,
  onSubmit,
}: Props) => {
  const { capabilities } = definition;

  const { updateForm, submitForms } = useChartBuilderFormsContext();

  // Prevent legend items from being a dependency of
  // `initialValues` by accessing it as a ref.
  // We basically do this to avoid the form
  // re-initialising whenever any of the items change.
  const legendItems = useRef(legend.items);
  legendItems.current = legend.items;

  const initialValues = useMemo<ChartLegendFormValues>(() => {
    const dataSetCategories: DataSetCategory[] = createDataSetCategories({
      axisConfiguration: {
        ...axisMajor,
        groupBy: definition.axes.major?.constants?.groupBy ?? axisMajor.groupBy,
      },
      data,
      meta,
    });

    const dataSetCategoryConfigs = getDataSetCategoryConfigs({
      dataSetCategories,
      groupByFilterGroups: axisMajor.groupByFilterGroups,
      legendItems: legendItems.current,
      meta,
    });

    const defaultConfig: Partial<LegendItemConfiguration> = {
      symbol: capabilities.hasSymbols ? 'none' : undefined,
      lineStyle: capabilities.hasLineStyle ? 'solid' : undefined,
      inlinePosition: capabilities.canPositionLegendInline
        ? 'right'
        : undefined,
    };

    const items = dataSetCategoryConfigs.map(({ config, rawDataSet }) => ({
      ...defaultConfig,
      ...config,
      dataSet: rawDataSet,
    }));

    return {
      position: legend.position,
      items,
    };
  }, [
    axisMajor,
    definition.axes.major?.constants?.groupBy,
    data,
    meta,
    legend.position,
    capabilities.canPositionLegendInline,
    capabilities.hasSymbols,
    capabilities.hasLineStyle,
  ]);

  const validationSchema = useMemo<ObjectSchema<ChartLegendFormValues>>(() => {
    let itemSchema: ObjectSchema<LegendItem> = Yup.object({
      dataSet: Yup.mixed<DataSet>().defined(),
      colour: Yup.string().required(
        params =>
          `Enter colour for legend item ${getLegendItemNumber(
            params.path as string,
          )}`,
      ),
      label: Yup.string().required(
        params =>
          `Enter label for legend item ${getLegendItemNumber(
            params.path as string,
          )}`,
      ),
      symbol: Yup.string<ChartSymbol>().optional(),
      lineStyle: Yup.string<LineStyle>().optional(),
      inlinePosition: Yup.string<LegendInlinePosition>().optional(),
    });

    if (capabilities.canPositionLegendInline) {
      itemSchema = itemSchema.shape({
        inlinePosition: Yup.string().oneOf<LegendInlinePosition>(
          ['above', 'below', 'right'],
          params =>
            `Choose a valid position for legend item ${getLegendItemNumber(
              params.path as string,
            )}`,
        ),
      });
    }

    if (capabilities.hasLineStyle) {
      itemSchema = itemSchema.shape({
        lineStyle: Yup.string().oneOf<LineStyle>(
          ['solid', 'dashed', 'dotted'],
          params =>
            `Choose a valid style for legend item ${getLegendItemNumber(
              params.path as string,
            )}`,
        ),
      });
    }

    if (capabilities.hasSymbols) {
      itemSchema = itemSchema.shape({
        symbol: Yup.string().oneOf<ChartSymbol>(
          [
            'circle',
            'cross',
            'diamond',
            'square',
            'star',
            'triangle',
            'wye',
            'none',
          ],
          params =>
            `Choose a valid symbol for legend item ${getLegendItemNumber(
              params.path as string,
            )}`,
        ),
      });
    }

    let baseSchema: ObjectSchema<ChartLegendFormValues> = Yup.object({
      items: Yup.array().defined().of(itemSchema),
      position: Yup.string<LegendPosition>().optional(),
    });

    if (capabilities.hasLegendPosition) {
      baseSchema = baseSchema.shape({
        position: Yup.string()
          .required('Select a legend position')
          .oneOf<LegendPosition>(
            ['none', 'bottom', 'top', 'inline'],
            'Select a valid legend position',
          )
          .test({
            name: 'noInlineWithDataLabels',
            message: 'Inline legends cannot be used with data labels',
            test(value) {
              return !(showDataLabels && value === 'inline');
            },
          }),
      });
    }

    return baseSchema;
  }, [
    capabilities.canPositionLegendInline,
    capabilities.hasLegendPosition,
    capabilities.hasLineStyle,
    capabilities.hasSymbols,
    showDataLabels,
  ]);

  const handleChange = useCallback(
    (values: LegendConfiguration) => {
      if (validationSchema.isValidSync(values)) {
        onChange(values);
      }
    },
    [onChange, validationSchema],
  );

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialValues}
      mode="onBlur"
      validationSchema={validationSchema}
    >
      {({ formState, watch }) => {
        const values = watch();
        return (
          <Form
            id={formId}
            onSubmit={async () => {
              onSubmit(values);
              await submitForms();
            }}
          >
            <Effect
              value={values.position}
              onChange={() => handleChange(values)}
            />

            <Effect
              value={{
                formKey: 'legend',
                isValid: formState.isValid,
                submitCount: formState.submitCount,
              }}
              onChange={updateForm}
              onMount={updateForm}
            />

            {validationSchema.fields.position && (
              <FormFieldSelect<ChartLegendFormValues>
                name="position"
                hint={
                  capabilities.canPositionLegendInline && showDataLabels
                    ? 'The legend cannot be positioned inline when data labels are used.'
                    : undefined
                }
                label="Legend position"
                options={
                  capabilities.canPositionLegendInline
                    ? positionOptions
                    : positionOptions.filter(
                        option => option.value !== 'inline',
                      )
                }
                order={FormSelect.unordered}
              />
            )}

            <h4>Legend items</h4>

            <ChartLegendItems
              capabilities={capabilities}
              position={values.position}
              onChange={onChange}
            />

            <ChartBuilderSaveActions
              formId={formId}
              formKey="legend"
              disabled={formState.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default ChartLegendConfiguration;

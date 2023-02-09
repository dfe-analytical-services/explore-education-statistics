import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import styles from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration.module.scss';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldSelect,
  FormFieldset,
  FormFieldTextInput,
} from '@common/components/form';
import FormFieldColourInput from '@common/components/form/FormFieldColourInput';
import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartSymbol,
  LineStyle,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import {
  LegendConfiguration,
  LegendItem,
  LegendItemConfiguration,
  LegendPosition,
  LegendInlinePosition,
} from '@common/modules/charts/types/legend';
import {
  legendInlinePositions,
  colours,
  legendPositions,
  lineStyles,
  symbols,
} from '@common/modules/charts/util/chartUtils';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import Yup from '@common/validation/yup';
import { Formik, FormikTouched } from 'formik';
import get from 'lodash/get';
import mapValues from 'lodash/mapValues';
import omit from 'lodash/omit';
import toPath from 'lodash/toPath';
import upperFirst from 'lodash/upperFirst';
import React, { ReactNode, useCallback, useMemo, useRef } from 'react';
import { ObjectSchema } from 'yup';

const positionOptions: SelectOption[] = legendPositions.map(position => ({
  label: upperFirst(position),
  value: position,
}));

const symbolOptions: SelectOption[] = symbols.map(symbol => ({
  label: upperFirst(symbol),
  value: symbol,
}));

const lineStyleOptions: SelectOption[] = lineStyles.map(lineStyle => ({
  label: upperFirst(lineStyle),
  value: lineStyle,
}));

const inlinePositionOptions: SelectOption[] = legendInlinePositions.map(
  position => ({
    label: upperFirst(position),
    value: position,
  }),
);

function getLegendItemNumber(path: string): number {
  const [, index] = toPath(path);
  return +index + 1;
}

type FormValues = LegendConfiguration;

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

  const {
    hasSubmitted,
    updateForm,
    submitForms,
  } = useChartBuilderFormsContext();

  // Prevent legend items from being a dependency of
  // `initialValues` by accessing it as a ref.
  // We basically do this to avoid the form
  // re-initialising whenever any of the items change.
  const legendItems = useRef(legend.items);
  legendItems.current = legend.items;

  const initialValues = useMemo<FormValues>(() => {
    const dataSetCategories: DataSetCategory[] = createDataSetCategories(
      {
        ...axisMajor,
        groupBy: definition.axes.major?.constants?.groupBy ?? axisMajor.groupBy,
      },
      data,
      meta,
    );

    const dataSetCategoryConfigs = getDataSetCategoryConfigs(
      dataSetCategories,
      legendItems.current,
      meta,
    );

    const defaultConfig: Partial<LegendItemConfiguration> = {
      symbol: capabilities.hasSymbols ? 'none' : undefined,
      lineStyle: capabilities.hasLineStyle ? 'solid' : undefined,
      inlinePosition: capabilities.canPositionLegendInline
        ? 'above'
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

  const initialTouched = useMemo<FormikTouched<FormValues> | undefined>(() => {
    if (!hasSubmitted) {
      return undefined;
    }

    return {
      position: true,
      items: initialValues.items.map(item =>
        mapValues(omit(item, 'dataSet'), () => true),
      ),
    };
  }, [hasSubmitted, initialValues.items]);

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    let itemSchema = Yup.object<LegendItem>({
      dataSet: Yup.object(),
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
    });

    if (capabilities.canPositionLegendInline) {
      itemSchema = itemSchema.shape({
        inlinePosition: Yup.string().oneOf<LegendInlinePosition>(
          ['above', 'below'],
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

    let baseSchema = Yup.object<FormValues>({
      items: Yup.array().of(itemSchema),
    });

    if (capabilities.hasLegendPosition) {
      baseSchema = baseSchema.shape({
        position: Yup.string()
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
    values => {
      if (validationSchema.isValidSync(values)) {
        onChange(values);
      }
    },
    [onChange, validationSchema],
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialValues}
      initialTouched={initialTouched}
      validateOnMount
      validationSchema={validationSchema}
      onSubmit={async values => {
        onSubmit(values);
        await submitForms();
      }}
    >
      {form => (
        <Form id={formId}>
          <Effect
            value={form.values}
            onChange={handleChange}
            onMount={
              // Update chart builder state with legend items if the
              // chart is using deprecated chart data set configurations
              form.values.items.length > 0 && legend.items.length === 0
                ? handleChange
                : undefined
            }
          />

          <Effect
            value={{
              formKey: 'legend',
              isValid: form.isValid,
              submitCount: form.submitCount,
            }}
            onChange={updateForm}
            onMount={updateForm}
          />

          {validationSchema.fields.position && (
            <>
              <FormFieldSelect<FormValues>
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
            </>
          )}

          <h4>Legend items</h4>
          <div className="dfe-overflow-x--auto govuk-!-margin-bottom-6">
            {form.values.items?.length > 0 ? (
              <>
                {form.values.items?.map((dataSet, index) => {
                  const itemId = `items-${index}`;
                  const itemName = `items[${index}]`;

                  const itemErrors: string[] = Object.values(
                    get(form.errors, itemName, {}),
                  );

                  return (
                    <div key={itemId} className={styles.item}>
                      <FormFieldset
                        id={itemId}
                        legend={`Legend item ${index + 1}`}
                        legendHidden
                        error={itemErrors[0]}
                      >
                        <div className="dfe-flex dfe-justify-content--space-between">
                          <div className={styles.labelInput}>
                            <FormFieldTextInput
                              name={`${itemName}.label`}
                              label="Label"
                              formGroup={false}
                              showError={false}
                            />
                          </div>

                          <div className={styles.colourInput}>
                            <FormFieldColourInput
                              name={`${itemName}.colour`}
                              label="Colour"
                              colours={colours}
                              formGroup={false}
                              showError={false}
                            />
                          </div>

                          {capabilities.hasSymbols && (
                            <div className={styles.configurationInput}>
                              <FormFieldSelect
                                name={`${itemName}.symbol`}
                                label="Symbol"
                                formGroup={false}
                                showError={false}
                                options={symbolOptions}
                              />
                            </div>
                          )}

                          {capabilities.hasLineStyle && (
                            <div className={styles.configurationInput}>
                              <FormFieldSelect
                                name={`${itemName}.lineStyle`}
                                label="Style"
                                order={FormSelect.unordered}
                                formGroup={false}
                                showError={false}
                                options={lineStyleOptions}
                              />
                            </div>
                          )}

                          {form.values.position === 'inline' && (
                            <div className={styles.configurationInput}>
                              <FormFieldSelect
                                name={`${itemName}.inlinePosition`}
                                label="Position"
                                order={FormSelect.unordered}
                                formGroup={false}
                                showError={false}
                                options={inlinePositionOptions}
                              />
                            </div>
                          )}
                        </div>
                      </FormFieldset>
                    </div>
                  );
                })}
              </>
            ) : (
              <p>No legend items to customize.</p>
            )}
          </div>

          <ChartBuilderSaveActions
            formId={formId}
            formKey="legend"
            disabled={form.isSubmitting}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </Form>
      )}
    </Formik>
  );
};

export default ChartLegendConfiguration;

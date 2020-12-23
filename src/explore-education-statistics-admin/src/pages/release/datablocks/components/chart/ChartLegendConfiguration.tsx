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
} from '@common/modules/charts/types/legend';
import {
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
  onChange,
  onSubmit,
}: Props) => {
  const { capabilities } = definition;

  const { hasSubmitted, updateForm, submit } = useChartBuilderFormsContext();

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
      symbol: capabilities.hasSymbols ? 'circle' : undefined,
      lineStyle: capabilities.hasLineStyle ? 'solid' : undefined,
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
        mapValues(item, () => true),
      ) as FormikTouched<LegendItem>[],
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
          ['circle', 'cross', 'diamond', 'square', 'star', 'triangle', 'wye'],
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
        position: Yup.string().oneOf<LegendPosition>(
          ['none', 'bottom', 'top'],
          'Select a valid legend position',
        ),
      });
    }

    return baseSchema;
  }, [
    capabilities.hasLegendPosition,
    capabilities.hasLineStyle,
    capabilities.hasSymbols,
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
      onSubmit={values => {
        onSubmit(values);
        submit();
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
            <FormFieldSelect<FormValues>
              id={`${formId}-position`}
              name="position"
              label="Position"
              options={positionOptions}
              order={FormSelect.unordered}
            />
          )}

          <div className="govuk-!-margin-bottom-6">
            <h4>Legend items</h4>

            {form.values.items?.length > 0 ? (
              <>
                {form.values.items?.map((dataSet, index) => {
                  const itemId = `${formId}-items${index}`;
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
                              id={`${itemId}Label`}
                              name={`${itemName}.label`}
                              label="Label"
                              formGroup={false}
                              showError={false}
                            />
                          </div>

                          <div className={styles.colourInput}>
                            <FormFieldColourInput
                              id={`${itemId}Colour`}
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
                                id={`${itemId}Symbol`}
                                name={`${itemName}.symbol`}
                                label="Symbol"
                                placeholder="None"
                                formGroup={false}
                                showError={false}
                                options={symbolOptions}
                              />
                            </div>
                          )}

                          {capabilities.hasLineStyle && (
                            <div className={styles.configurationInput}>
                              <FormFieldSelect
                                id={`${itemId}LineStyle`}
                                name={`${itemName}.lineStyle`}
                                label="Style"
                                order={FormSelect.unordered}
                                formGroup={false}
                                showError={false}
                                options={lineStyleOptions}
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

          <ChartBuilderSaveActions formId={formId} formKey="legend">
            {buttons}
          </ChartBuilderSaveActions>
        </Form>
      )}
    </Formik>
  );
};

export default ChartLegendConfiguration;

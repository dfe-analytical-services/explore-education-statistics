import ChartCustomDataGroupingsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartCustomDataGroupingsConfiguration';
import styles from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm.module.scss';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import {
  CustomDataGroup,
  DataGroupingConfig,
  DataGroupingType,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import { maxMapDataGroups } from '@common/modules/charts/components/MapBlock';

const formId = 'chartDataGroupingForm';

type GroupingType = DataGroupingType | 'CopyCustom';

export interface ChartDataGroupingFormValues {
  customGroups?: CustomDataGroup[];
  numberOfGroups?: number;
  numberOfGroupsQuantiles?: number;
  copyCustomGroups?: string;
  type: GroupingType;
  min?: number;
  max?: number;
}

interface Props {
  dataSetConfig: MapDataSetConfig;
  dataSetConfigs: MapDataSetConfig[];
  meta: FullTableMeta;
  unit?: string;
  onCancel: () => void;
  onSubmit: (values: MapDataSetConfig) => void;
}

export default function ChartDataGroupingForm({
  dataSetConfig,
  dataSetConfigs,
  meta,
  unit,
  onCancel,
  onSubmit,
}: Props) {
  const dataSetsWithCustomGroupsOptions = useMemo(() => {
    const dataGroupingKey = generateDataSetKey(dataSetConfig.dataSet);
    return dataSetConfigs
      .filter(config => config.dataGrouping.type === 'Custom')
      .map(config => {
        const expandedDataSet = expandDataSet(config.dataSet, meta);
        return {
          ...config,
          label: generateDataSetLabel(expandedDataSet),
          value: generateDataSetKey(config.dataSet),
        };
      })
      .filter(grouping => grouping.value !== dataGroupingKey);
  }, [dataSetConfigs, dataSetConfig.dataSet, meta]);

  const validationSchema = useMemo<
    ObjectSchema<ChartDataGroupingFormValues>
  >(() => {
    return Yup.object({
      customGroups: Yup.array()
        .of(
          Yup.object({
            min: Yup.number().required(),
            max: Yup.number().required(),
          }),
        )
        .when('type', {
          is: 'Custom',
          then: s =>
            s
              .required('Add one or more custom groups')
              .test(
                'at-least-one',
                'Add one or more custom groups',
                function atLeastOne(value) {
                  return value && value.length > 0;
                },
              )
              .test(
                'max-five',
                `Maximum ${maxMapDataGroups} custom groups`,
                function maxFive(value) {
                  return (
                    value &&
                    value.length > 0 &&
                    value.length <= maxMapDataGroups
                  );
                },
              ),
        }),

      numberOfGroups: Yup.number()
        .max(maxMapDataGroups, `Maximum ${maxMapDataGroups} data groups`)
        .min(1, 'Minimum 1 data group')
        .when('type', {
          is: 'EqualIntervals',
          then: s => s.required('Enter a number of data groups'),

          otherwise: s => s.notRequired(),
        }),
      numberOfGroupsQuantiles: Yup.number()
        .max(maxMapDataGroups, `Maximum ${maxMapDataGroups} data groups`)
        .min(1, 'Minimum 1 data group')
        .when('type', {
          is: 'Quantiles',
          then: s => s.required('Enter a number of data groups'),
          otherwise: s => s.notRequired(),
        }),
      copyCustomGroups: Yup.string().when('type', {
        is: 'CopyCustom',
        then: s => s.required('Select a data set to copy custom groups from'),
        otherwise: s => s.notRequired(),
      }),
      type: Yup.string()
        .oneOf<GroupingType>([
          'EqualIntervals',
          'Quantiles',
          'Custom',
          'CopyCustom',
        ])
        .required(),

      min: Yup.number().test(
        'noOverlap',
        'Min cannot overlap another group',
        function noOverlap(value?: number) {
          // eslint-disable-next-line react/no-this-in-sfc
          const groups: CustomDataGroup[] = this.parent.customGroups ?? [];

          // Show error when the min is in an existing group
          if (
            value !== undefined &&
            groups.some(group => value >= group.min && value <= group.max)
          ) {
            return false;
          }

          /* eslint-disable react/no-this-in-sfc */
          // This group overlaps with an existing group
          const overlaps =
            value !== undefined &&
            groups.some(
              group => value <= group.max && group.min <= this.parent.max,
            );

          if (overlaps) {
            // Check if the max is in an existing group,
            // don't show the error here if it is as only want
            // to show the error on the field(s) that require action.
            return groups.some(
              group =>
                this.parent.max >= group.min && this.parent.max <= group.max,
            );
          }
          /* eslint-enable react/no-this-in-sfc */
          return true;
        },
      ),
      max: Yup.number()
        .moreThan(Yup.ref('min'), 'Must be greater than min')
        .test(
          'noOverlap',
          'Max cannot overlap another group',
          function noOverlap(value?: number) {
            // eslint-disable-next-line react/no-this-in-sfc
            const groups: CustomDataGroup[] = this.parent.customGroups ?? [];

            // Show error when the max is in an existing group
            if (
              value !== undefined &&
              groups.some(group => value >= group.min && value <= group.max)
            ) {
              return false;
            }

            /* eslint-disable react/no-this-in-sfc */
            // This group overlaps with an existing group
            const overlaps =
              value !== undefined &&
              groups.some(
                group => this.parent.min <= group.max && group.min <= value,
              );

            if (overlaps) {
              // Check if the min is in an existing group,
              // don't show the error here if it is as only want
              // to show the error on the field(s) that require action.
              return groups.some(
                group =>
                  this.parent.min >= group.min && this.parent.min <= group.max,
              );
            }
            /* eslint-enable react/no-this-in-sfc */
            return true;
          },
        ),
    });
  }, []);
  return (
    <FormProvider
      initialValues={{
        ...dataSetConfig.dataGrouping,
        numberOfGroups:
          dataSetConfig.dataGrouping.type === 'EqualIntervals'
            ? dataSetConfig.dataGrouping.numberOfGroups
            : maxMapDataGroups,
        numberOfGroupsQuantiles:
          dataSetConfig.dataGrouping.type === 'Quantiles'
            ? dataSetConfig.dataGrouping.numberOfGroups
            : maxMapDataGroups,
        copyCustomGroups: undefined,
      }}
      mode="onBlur"
      validationSchema={validationSchema}
    >
      {({ resetField, setValue, watch }) => {
        const values = watch();
        const defaultOptions: RadioOption<GroupingType>[] = [
          {
            label: 'Equal intervals',
            value: 'EqualIntervals',
            hint: `Data is grouped into equal-sized ranges. Maximum ${maxMapDataGroups} groups.`,
            conditional: (
              <FormFieldNumberInput<ChartDataGroupingFormValues>
                name="numberOfGroups"
                label="Number of data groups"
                width={3}
              />
            ),
          },
          {
            label: 'Quantiles',
            value: 'Quantiles',
            hint: `Data is grouped so that each group has a similar number of data points. Maximum ${maxMapDataGroups} groups.`,
            conditional: (
              <FormFieldNumberInput<ChartDataGroupingFormValues>
                name="numberOfGroupsQuantiles"
                label="Number of data groups"
                width={3}
              />
            ),
          },

          {
            label: 'New custom groups',
            value: 'Custom',
            hint: `Define custom groups. Maximum ${maxMapDataGroups} groups.`,
            conditional: (
              <ChartCustomDataGroupingsConfiguration
                groups={values.customGroups}
                id={`${formId}-customDataGroups`}
                unit={unit}
                onAddGroup={() => {
                  if (values.min !== undefined && values.max !== undefined) {
                    const currentGroups = values.customGroups ?? [];
                    const group: CustomDataGroup = {
                      min: values.min,
                      max: values.max,
                    };

                    setValue('customGroups' as const, [
                      ...currentGroups,
                      group,
                    ]);

                    resetField('min');
                    resetField('max');
                  }
                }}
                onRemoveGroup={group => {
                  setValue(
                    'customGroups' as const,
                    values.customGroups?.filter(
                      g => !(g.min === group.min && g.max === group.max),
                    ),
                  );
                }}
              />
            ),
          },
        ];

        const copyCustomOption: RadioOption<GroupingType> = {
          label: 'Copy custom groups',
          value: 'CopyCustom',
          hint: 'Copy custom groups from another data set.',
          conditional: (
            <FormFieldSelect<ChartDataGroupingFormValues>
              name="copyCustomGroups"
              className={styles.selectContainer}
              label="Copy custom groups from another data set"
              hideLabel
              options={dataSetsWithCustomGroupsOptions.map(group => ({
                label: group.label,
                value: group.value,
              }))}
              placeholder="Select a data set"
            />
          ),
        };

        return (
          <div className={styles.container}>
            <Form
              id={formId}
              onSubmit={() => {
                const copiedCustomGroups: CustomDataGroup[] =
                  values.type === 'CopyCustom'
                    ? dataSetsWithCustomGroupsOptions.find(
                        grouping => grouping.value === values.copyCustomGroups,
                      )?.dataGrouping.customGroups ?? []
                    : [];

                const updatedGrouping: DataGroupingConfig = {
                  customGroups:
                    values.type === 'CopyCustom'
                      ? copiedCustomGroups
                      : values.customGroups ?? [],
                  type: values.type === 'CopyCustom' ? 'Custom' : values.type,
                };

                onSubmit({
                  dataSet: dataSetConfig.dataSet,
                  dataSetKey: 'dataSetKey1',
                  dataGrouping:
                    updatedGrouping.type === 'Custom'
                      ? updatedGrouping
                      : {
                          ...updatedGrouping,
                          numberOfGroups:
                            values.type === 'Quantiles' &&
                            values.numberOfGroupsQuantiles
                              ? values.numberOfGroupsQuantiles
                              : values.numberOfGroups,
                        },
                });
              }}
            >
              <FormFieldRadioGroup<ChartDataGroupingFormValues>
                legend="Select a grouping type"
                legendSize="s"
                name="type"
                options={
                  dataSetsWithCustomGroupsOptions.length
                    ? [...defaultOptions, copyCustomOption]
                    : defaultOptions
                }
                order={[]}
              />
              <ButtonGroup>
                <Button type="submit">Done</Button>
                <Button onClick={onCancel} variant="secondary">
                  Cancel
                </Button>
              </ButtonGroup>
            </Form>
          </div>
        );
      }}
    </FormProvider>
  );
}

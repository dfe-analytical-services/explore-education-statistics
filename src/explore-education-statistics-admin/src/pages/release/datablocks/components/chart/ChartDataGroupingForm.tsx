import ChartCustomDataGroupingsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartCustomDataGroupingsConfiguration';
import styles from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm.module.scss';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldSelect,
} from '@common/components/form';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
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
import { Formik } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';
import generateDataSetLabel from './utils/generateDataSetLabel';

const formId = 'chartDataGroupingForm';

type GroupingType = DataGroupingType | 'CopyCustom';

interface FormValues {
  customGroups: CustomDataGroup[];
  numberOfGroups?: number;
  numberOfGroupsQuantiles?: number;
  copyCustomGroups?: string;
  type: GroupingType;
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

  return (
    <Formik<FormValues>
      initialValues={{
        ...dataSetConfig.dataGrouping,
        numberOfGroupsQuantiles: dataSetConfig.dataGrouping.numberOfGroups,
        copyCustomGroups: undefined,
      }}
      validationSchema={Yup.object<FormValues>({
        customGroups: Yup.array()
          .of(
            Yup.object<CustomDataGroup>({
              min: Yup.number().required(),
              max: Yup.number().required(),
            }),
          )
          .when('type', {
            is: 'Custom',
            then: Yup.array().required('Add one or more custom groups'),
          }),
        numberOfGroups: Yup.number().when('type', {
          is: 'EqualIntervals',
          then: Yup.number().required('Enter a number of data groups'),
          otherwise: Yup.number(),
        }),
        numberOfGroupsQuantiles: Yup.number().when('type', {
          is: 'Quantiles',
          then: Yup.number().required('Enter a number of data groups'),
          otherwise: Yup.number(),
        }),
        copyCustomGroups: Yup.string().when('type', {
          is: 'CopyCustom',
          then: Yup.string().required(
            'Select a data set to copy custom groups from',
          ),
        }),
        type: Yup.string()
          .oneOf<GroupingType>([
            'EqualIntervals',
            'Quantiles',
            'Custom',
            'CopyCustom',
          ])
          .required(),
      })}
      onSubmit={values => {
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
              : values.customGroups,
          type: values.type === 'CopyCustom' ? 'Custom' : values.type,
        };

        onSubmit({
          dataSet: dataSetConfig.dataSet,
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
      {form => {
        const defaultOptions: RadioOption<GroupingType>[] = [
          {
            label: 'Equal intervals',
            value: 'EqualIntervals',
            hint: 'Data is grouped into equal-sized ranges.',
            conditional: (
              <FormFieldNumberInput<FormValues>
                name="numberOfGroups"
                label="Number of data groups"
                width={3}
              />
            ),
          },
          {
            label: 'Quantiles',
            value: 'Quantiles',
            hint:
              'Data is grouped so that each group has a similar number of data points.',
            conditional: (
              <FormFieldNumberInput<FormValues>
                name="numberOfGroupsQuantiles"
                label="Number of data groups"
                width={3}
              />
            ),
          },

          {
            label: 'New custom groups',
            value: 'Custom',
            hint: 'Define custom groups.',
            conditional: (
              <ChartCustomDataGroupingsConfiguration
                groups={form.values.customGroups}
                id={`${formId}-customDataGroups`}
                unit={unit}
                onAddGroup={group => {
                  form.setFieldValue(
                    'customGroups',
                    orderBy([...form.values.customGroups, group], g => g.min),
                  );
                }}
                onRemoveGroup={group => {
                  form.setFieldValue(
                    'customGroups',
                    form.values.customGroups?.filter(
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
            <FormFieldSelect<FormValues>
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
            <Form id={formId}>
              <FormFieldRadioGroup<FormValues>
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
    </Formik>
  );
}

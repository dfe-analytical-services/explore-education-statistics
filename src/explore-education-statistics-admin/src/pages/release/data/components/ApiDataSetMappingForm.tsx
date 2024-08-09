import {
  FilterCandidateWithKey,
  FilterMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import styles from '@admin/pages/release/data/components/ApiDataSetMappingModal.module.scss';
import { PendingMappingUpdate } from '@admin/services/apiDataSetVersionService';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import getApiDataSetLocationCodes from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import Yup from '@common/validation/yup';
import React from 'react';

const noMappingValue = 'noMapping';

interface FormValues {
  nextItem: string;
}

interface Props {
  candidate?: FilterCandidateWithKey | LocationCandidateWithKey;
  groupKey: string;
  label: string;
  mapping: FilterMappingWithKey | LocationMappingWithKey;
  newItems: FilterCandidateWithKey[] | LocationCandidateWithKey[];
  onCancel?: () => void;
  onSubmit: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetMappingForm({
  candidate,
  groupKey,
  label,
  mapping,
  newItems = [],
  onCancel,
  onSubmit,
}: Props) {
  const handleSubmit = async ({ nextItem }: FormValues) => {
    await onSubmit({
      candidateKey: nextItem !== noMappingValue ? nextItem : undefined,
      groupKey,
      sourceKey: mapping.sourceKey,
      type: nextItem !== noMappingValue ? 'ManualMapped' : 'ManualNone',
      previousCandidate: candidate,
      previousMapping: mapping,
    });
  };

  const options: RadioOption<string>[] = [
    {
      label: 'No mapping available',
      value: noMappingValue,
      divider: `Select a ${label}:`,
    },
    ...(candidate
      ? [
          {
            label: candidate?.label,
            value: candidate.key,
            hint: getApiDataSetLocationCodes(candidate),
          },
        ]
      : []),
    ...newItems.map(option => {
      return {
        label: option.label,
        value: option.key,
        hint: getApiDataSetLocationCodes(option),
      };
    }),
  ];

  return (
    <FormProvider
      initialValues={getInitialValues({ candidate, mapping })}
      validationSchema={Yup.object({
        nextItem: Yup.string().required(`Select the next data set ${label}`),
      })}
    >
      {({ formState }) => {
        return (
          <Form id={`map-${groupKey}-form`} onSubmit={handleSubmit}>
            <FormFieldRadioSearchGroup<FormValues>
              alwaysShowOptions={[noMappingValue]}
              hint={`Choose a ${label} that will be mapped to the current data set ${label} (see above).`}
              legend={`Next data set ${label}`}
              name="nextItem"
              options={options}
              order={[]}
              searchLabel={`Search ${label}s`}
              small
            />
            <ButtonGroup className={styles.buttons}>
              <Button disabled={formState.isSubmitting} type="submit">
                {`Update ${label} mapping`}
              </Button>
              <ButtonText disabled={formState.isSubmitting} onClick={onCancel}>
                Cancel
              </ButtonText>

              <LoadingSpinner
                alert
                hideText
                inline
                loading={formState.isSubmitting}
                size="sm"
                text={`Updating ${label} mapping`}
              />
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}

function getInitialValues({
  candidate,
  mapping,
}: {
  candidate?: FilterCandidateWithKey | LocationCandidateWithKey;
  mapping: FilterMappingWithKey | LocationMappingWithKey;
}) {
  if (candidate) {
    return { nextItem: candidate.key };
  }
  if (mapping.type === 'ManualNone') {
    return { nextItem: noMappingValue };
  }
  return undefined;
}

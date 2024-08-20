import {
  FilterOptionCandidateWithKey,
  FilterOptionMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import styles from '@admin/pages/release/data/components/ApiDataSetMappingModal.module.scss';
import { PendingMappingUpdate } from '@admin/pages/release/data/types/apiDataSetMappings';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
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
  candidate?: FilterOptionCandidateWithKey | LocationCandidateWithKey;
  candidateHint?: (
    candidate: FilterOptionCandidateWithKey | LocationCandidateWithKey,
  ) => string;
  groupKey: string;
  itemLabel: string;
  mapping: FilterOptionMappingWithKey | LocationMappingWithKey;
  newItems: FilterOptionCandidateWithKey[] | LocationCandidateWithKey[];
  onCancel?: () => void;
  onSubmit: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetMappingForm({
  candidate,
  candidateHint,
  groupKey,
  itemLabel,
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
      divider: `Select a ${itemLabel}:`,
    },
    ...(candidate
      ? [
          {
            label: candidate?.label,
            value: candidate.key,
            hint: candidateHint?.(candidate),
          },
        ]
      : []),
    ...newItems.map(item => {
      return {
        label: item.label,
        value: item.key,
        hint: candidateHint?.(item),
      };
    }),
  ];

  return (
    <FormProvider
      initialValues={getInitialValues({ candidate, mapping })}
      validationSchema={Yup.object({
        nextItem: Yup.string().required(
          `Select the next data set ${itemLabel}`,
        ),
      })}
    >
      {({ formState }) => {
        return (
          <Form id={`mapping-${groupKey}-form`} onSubmit={handleSubmit}>
            <FormFieldRadioSearchGroup<FormValues>
              alwaysShowOptions={[noMappingValue]}
              hint={`Choose a ${itemLabel} that will be mapped to the current data set ${itemLabel} (see above).`}
              legend={`Next data set ${itemLabel}`}
              name="nextItem"
              options={options}
              order={[]}
              searchLabel={`Search ${itemLabel}s`}
              small
            />
            <ButtonGroup className={styles.buttons}>
              <Button disabled={formState.isSubmitting} type="submit">
                {`Update ${itemLabel} mapping`}
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
                text={`Updating ${itemLabel} mapping`}
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
  candidate?: FilterOptionCandidateWithKey | LocationCandidateWithKey;
  mapping: FilterOptionMappingWithKey | LocationMappingWithKey;
}) {
  if (candidate) {
    return { nextItem: candidate.key };
  }
  if (mapping.type === 'ManualNone') {
    return { nextItem: noMappingValue };
  }
  return undefined;
}

import {
  FilterOptionCandidateWithKey,
  FilterOptionMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import styles from '@admin/pages/release/data/components/ApiDataSetMappingModal.module.scss';
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

export interface NewCandidate {
  label: string;
  value: string;
  hint?: string;
}

const noMappingValue = 'noMapping';

export interface ApiDataSetMappingFormValues {
  candidateKey: string;
}

interface Props<TMapping> {
  candidates: NewCandidate[];
  itemLabel: string;
  mapping: TMapping;
  onCancel?: () => void;
  onSubmit: (values: ApiDataSetMappingFormValues) => Promise<void>;
}

export default function ApiDataSetMappingForm<TMapping>({
  candidates = [],
  itemLabel,
  mapping,
  onCancel,
  onSubmit,
}: Props<TMapping>) {
  const options: RadioOption<string>[] = [
    {
      label: 'No mapping available',
      value: noMappingValue,
      divider: `Select a ${itemLabel}:`,
    },
    ...candidates,
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
          <Form id="apiDataSetMappingForm" onSubmit={onSubmit}>
            <FormFieldRadioSearchGroup<ApiDataSetMappingFormValues>
              alwaysShowOptions={[noMappingValue]}
              hint={`Choose a ${itemLabel} that will be mapped to the current data set ${itemLabel} (see above).`}
              legend={`Next data set ${itemLabel}`}
              name="candidateKey"
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

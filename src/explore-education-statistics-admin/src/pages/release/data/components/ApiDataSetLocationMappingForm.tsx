import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import {
  PendingLocationMappingUpdate,
  noMappingValue,
} from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import styles from '@admin/pages/release/data/components/ApiDataSetLocationMappingModal.module.scss';
import getApiDataSetLocationCodes from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import Yup from '@common/validation/yup';
import React from 'react';

interface FormValues {
  nextLocation: string;
}

interface Props {
  candidate?: LocationCandidateWithKey;
  level: LocationLevelKey;
  mapping: LocationMappingWithKey;
  newLocations: LocationCandidateWithKey[];
  onCancel?: () => void;
  onSubmit: (update: PendingLocationMappingUpdate) => Promise<void>;
}

export default function ApiDataSetLocationMappingForm({
  candidate,
  level,
  mapping,
  newLocations = [],
  onCancel,
  onSubmit,
}: Props) {
  const handleSubmit = async ({ nextLocation }: FormValues) => {
    await onSubmit({
      candidateKey: nextLocation !== noMappingValue ? nextLocation : undefined,
      level,
      sourceKey: mapping.sourceKey,
      type: nextLocation !== noMappingValue ? 'ManualMapped' : 'ManualNone',
      previousCandidate: candidate,
      previousMapping: mapping,
    });
  };

  const options: RadioOption<string>[] = [
    {
      label: 'No mapping available',
      value: noMappingValue,
      divider:
        newLocations.length || candidate ? 'Select a location:' : undefined,
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
    ...newLocations.map(location => {
      return {
        label: location.label,
        value: location.key,
        hint: getApiDataSetLocationCodes(location),
      };
    }),
  ];

  return (
    <FormProvider
      initialValues={getInitialValues({ candidate, mapping })}
      validationSchema={Yup.object({
        nextLocation: Yup.string().required(
          'Select the next data set location',
        ),
      })}
    >
      {({ formState }) => {
        return (
          <Form id="map-location-form" onSubmit={handleSubmit}>
            <FormFieldRadioSearchGroup<FormValues>
              alwaysShowOptions={[noMappingValue]}
              hint="Choose a location that will be mapped to the current data set location (see above)."
              legend="Next data set location"
              name="nextLocation"
              options={options}
              order={[]}
              searchLabel="Search locations"
              small
            />
            <ButtonGroup className={styles.buttons}>
              <Button disabled={formState.isSubmitting} type="submit">
                Update location mapping
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
                text="Updating location mapping"
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
  candidate?: LocationCandidateWithKey;
  mapping: LocationMappingWithKey;
}) {
  if (candidate) {
    return { nextLocation: candidate.key };
  }
  if (mapping.type === 'ManualNone') {
    return { nextLocation: noMappingValue };
  }
  return undefined;
}

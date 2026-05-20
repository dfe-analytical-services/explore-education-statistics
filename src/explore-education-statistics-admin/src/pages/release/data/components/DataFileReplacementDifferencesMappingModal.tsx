import FormModal from '@admin/components/FormModal';
import {
  IndicatorSource,
  LocationSource,
  MappingWithKey,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import ButtonText from '@common/components/ButtonText';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary, KeysWithType } from '@common/types';
import prefixNoun from '@common/utils/string/prefixNoun';
import Yup from '@common/validation/yup';
import React from 'react';

type ItemType = 'indicator' | 'location';

interface ModalProps<
  TItemType extends ItemType,
  TSourceItemType = LocationSource | IndicatorSource,
> {
  itemType: TItemType;
  allCandidateOptions: Dictionary<TSourceItemType>;
  unmappedCandidateOptions: Dictionary<TSourceItemType>;
  mapping: MappingWithKey<TSourceItemType>;
  onSubmit: (payload: UpdateMappingPayload) => void;
  label: KeysWithType<TSourceItemType, string>;
}

interface FormValues {
  selectedCandidate: string;
}

export default function DifferencesItemMappingModal<
  TItemType extends ItemType,
  TSourceItemType = LocationSource | IndicatorSource,
>({
  mapping,
  itemType,
  allCandidateOptions,
  unmappedCandidateOptions,
  onSubmit,
  label,
}: ModalProps<TItemType, TSourceItemType>) {
  const noMappingValue = 'noMapping';

  const currentCandidate = !mapping.candidateKey
    ? undefined
    : {
        key: mapping.candidateKey,
        ...allCandidateOptions[mapping.candidateKey],
      };

  const handleSubmit = async ({ selectedCandidate }: FormValues) => {
    onSubmit({
      sourceKey: mapping.sourceKey,
      candidateKey:
        selectedCandidate !== noMappingValue ? selectedCandidate : undefined,
    });
  };

  const options: RadioOption<string>[] = [
    {
      label: 'No mapping available',
      value: noMappingValue,
      divider: `Select ${itemType}:`,
    },
    ...(currentCandidate
      ? [
          {
            label: currentCandidate[label] as string,
            value: currentCandidate.key,
          },
        ]
      : []),
    ...Object.entries(unmappedCandidateOptions).map(
      ([candidateName, candidate]) => {
        return {
          label: candidate[label] as string,
          value: candidateName,
        };
      },
    ),
  ];

  const initialValue = {
    selectedCandidate: mapping.candidateKey ?? noMappingValue,
  };

  return (
    <FormModal
      formId="mapping-form"
      onSubmit={async submittedValue => {
        if (
          submittedValue.selectedCandidate === initialValue.selectedCandidate
        ) {
          return;
        }
        handleSubmit(submittedValue);
      }}
      initialValues={initialValue}
      validationSchema={Yup.object({
        selectedCandidate: Yup.string().required(
          `Select the next data set ${itemType}`,
        ),
      })}
      title={`Map existing ${itemType}`}
      triggerButton={
        <ButtonText className="govuk-!-margin-left-2">
          Map item{' '}
          <VisuallyHidden>{mapping.source[label] as string}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>Original data set {itemType}</h3>
      <SummaryList>
        <SummaryListItem term="Name">{mapping.sourceKey}</SummaryListItem>
        <SummaryListItem term="Label">
          {mapping.source[label] as string}
        </SummaryListItem>
      </SummaryList>

      <FormFieldRadioSearchGroup<FormValues>
        alwaysShowOptions={[noMappingValue]}
        hint={`Choose ${prefixNoun(
          itemType,
        )} ${itemType} that will be mapped to the current data set ${itemType} (see above).`}
        legend={`Next data set ${itemType}`}
        name="selectedCandidate"
        options={options}
        order={[]}
        searchLabel={`Search ${itemType}s`}
        small
      />
    </FormModal>
  );
}

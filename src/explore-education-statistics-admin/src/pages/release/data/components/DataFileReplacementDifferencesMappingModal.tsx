import FormModal from '@admin/components/FormModal';
import {
  ReplacementMapping,
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
import { TypeMapping } from '@admin/pages/release/data/components/DataFileReplacementDifferencesTable';

interface FormValues {
  selectedCandidate: string;
}

type SourceStringKey<ItemType extends keyof TypeMapping> = KeysWithType<
  TypeMapping[ItemType]['source'],
  string
>;

export type LabelProps<ItemType extends keyof TypeMapping> = {
  rowLabel: SourceStringKey<ItemType>;
  mappedDataLabels: Partial<Record<SourceStringKey<ItemType>, string>>;
};

export type DifferencesItemMappingModalProps<
  ItemType extends keyof TypeMapping,
  SourceItemType extends TypeMapping[ItemType]['source'],
> = {
  itemType: ItemType;
  allCandidateOptions: Dictionary<SourceItemType>;
  unmappedCandidateOptions: Dictionary<SourceItemType>;
  mapping: ReplacementMapping<SourceItemType>;
  onSubmit: (payload: UpdateMappingPayload) => void;
} & LabelProps<ItemType>;

export default function DifferencesItemMappingModal<
  ItemType extends keyof TypeMapping,
  SourceItemType extends TypeMapping[ItemType]['source'],
>({
  mapping,
  itemType,
  allCandidateOptions,
  unmappedCandidateOptions,
  onSubmit,
  rowLabel,
  mappedDataLabels,
}: DifferencesItemMappingModalProps<ItemType, SourceItemType>) {
  const noMappingValue = 'noMapping';

  const labelEntries = Object.entries(mappedDataLabels) as [
    keyof SourceItemType,
    string,
  ][];

  const hintLabelEntries = labelEntries.filter(([k]) => k !== rowLabel);

  const currentCandidate = !mapping.candidateKey
    ? undefined
    : {
        key: mapping.candidateKey,
        ...allCandidateOptions[mapping.candidateKey],
      };

  const handleSubmit = async ({ selectedCandidate }: FormValues) => {
    onSubmit({
      sourceKey: mapping.source.id,
      candidateKey:
        selectedCandidate !== noMappingValue ? selectedCandidate : undefined,
    });
  };

  const generateCandidateOption = ([id, candidate]: [
    string,
    SourceItemType,
  ]) => {
    return {
      value: id,
      label: candidate[rowLabel] as string,
      hint: hintLabelEntries
        ? `(${hintLabelEntries
            .map(([key, label]) => `${label} : ${candidate[key]}`)
            .join(', ')})`
        : undefined,
    };
  };

  const options: RadioOption[] = [
    {
      label: 'No mapping available',
      value: noMappingValue,
      divider: `Select ${itemType}:`,
    },
    ...(currentCandidate
      ? [generateCandidateOption([currentCandidate.key, currentCandidate])]
      : []),
    ...Object.entries(unmappedCandidateOptions).map(generateCandidateOption),
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
          <VisuallyHidden>{mapping.source[rowLabel] as string}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>Original data set {itemType}</h3>
      <SummaryList>
        {labelEntries.map(([key, label]) => (
          <SummaryListItem key={label} term={label}>
            {mapping.source[key] as string}
          </SummaryListItem>
        ))}
      </SummaryList>

      <FormFieldRadioSearchGroup<FormValues>
        alwaysShowOptions={[noMappingValue]}
        hint={`Choose ${prefixNoun(
          itemType,
        )} ${itemType} that will be mapped to the current data set ${itemType} (see above).`}
        legend={`Replacement data set ${itemType}`}
        name="selectedCandidate"
        options={options}
        order={[]}
        searchLabel={`Search ${itemType}s`}
        small
      />
    </FormModal>
  );
}

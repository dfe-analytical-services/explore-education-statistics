import FormModal from '@admin/components/FormModal';
import {
  Mapping,
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
import { TypeMapping } from '@admin/pages/release/data/components/DataFileReplacementTable';
import startCase from 'lodash/startCase';

type ItemType = 'indicator' | 'location';

interface FormValues {
  selectedCandidate: string;
}

export default function DifferencesItemMappingModal<
  TItemType extends ItemType,
  TSourceItemType extends TypeMapping[TItemType]['source'],
>({
  mapping,
  itemType,
  allCandidateOptions,
  unmappedCandidateOptions,
  onSubmit,
  mappedDataLabels,
}: {
  itemType: TItemType;
  allCandidateOptions: Dictionary<TSourceItemType>;
  unmappedCandidateOptions: Dictionary<TSourceItemType>;
  mapping: Mapping<TSourceItemType>;
  onSubmit: (payload: UpdateMappingPayload) => void;
  mappedDataLabels: KeysWithType<TSourceItemType, string>[];
}) {
  const noMappingValue = 'noMapping';
  const [mainLabel, ...otherLabels] = mappedDataLabels;

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
    TSourceItemType,
  ]) => {
    return {
      value: id,
      label: candidate[mainLabel] as string,
      hint: otherLabels?.length
        ? `(${otherLabels
            .map(item => `${String(item)} : ${candidate[item]}`)
            .join(', ')})`
        : undefined,
    };
  };

  const options: RadioOption<string>[] = [
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
          <VisuallyHidden>{mapping.source[mainLabel] as string}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>Original data set {itemType}</h3>
      <SummaryList>
        {mappedDataLabels.map(dataLabel => (
          <SummaryListItem
            key={String(dataLabel)}
            term={startCase(String(dataLabel))}
          >
            {mapping.source[dataLabel] as string}
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

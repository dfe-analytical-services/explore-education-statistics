import FormModal from '@admin/components/FormModal';
import {
  MappingWithKey,
  SourceItem,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import ButtonText from '@common/components/ButtonText';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import prefixNoun from '@common/utils/string/prefixNoun';
import Yup from '@common/validation/yup';
import React from 'react';

interface ModalProps {
  itemType: string;
  allCandidateOptions: Dictionary<SourceItem>;
  unmappedCandidateOptions: Dictionary<SourceItem>;
  mapping: MappingWithKey<SourceItem>;
  onSubmit: (payload: UpdateMappingPayload) => void;
}

interface FormValues {
  selectedCandidate: string;
}

export default function DifferencesItemMappingModal({
  mapping,
  itemType,
  allCandidateOptions,
  unmappedCandidateOptions,
  onSubmit,
}: ModalProps) {
  const noMappingValue = 'noMapping';

  const currentCandidate = !mapping.candidateKey
    ? undefined
    : {
        key: mapping.candidateKey,
        label: allCandidateOptions[mapping.candidateKey].label,
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
      ? [{ label: currentCandidate.label, value: currentCandidate.key }]
      : []),
    ...Object.entries(unmappedCandidateOptions).map(
      ([candidateName, candidate]) => {
        return {
          label: candidate.label,
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
          Map item <VisuallyHidden>{mapping.source.label}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>Current data set {itemType}</h3>
      <SummaryList>
        <SummaryListItem term="Name">{mapping.sourceKey}</SummaryListItem>
        <SummaryListItem term="Label">{mapping.source.label}</SummaryListItem>
      </SummaryList>

      <FormFieldRadioSearchGroup<FormValues>
        alwaysShowOptions={[noMappingValue]}
        hint={`Choose a ${prefixNoun(
          itemType,
        )} that will be mapped to the current data set ${itemType} (see above).`}
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

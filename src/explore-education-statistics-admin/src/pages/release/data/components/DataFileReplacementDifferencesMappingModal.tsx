import {
  MappingWithKey,
  SourceItem,
  UpdateMappingPayloadMultiple,
} from '@admin/services/dataReplacementService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import FormProvider from '@common/components/form/FormProvider';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import React from 'react';

interface ModalProps {
  itemType: string;
  allCandidateOptions: Dictionary<SourceItem>;
  unmappedCandidateOptions: Dictionary<SourceItem>;
  currentItem: MappingWithKey<SourceItem>;
  onSubmit: (payload: UpdateMappingPayloadMultiple) => void;
  onClose: () => void;
}

interface FormValues {
  selectedCandidate: string;
}

export default function DifferencesItemMappingModal({
  currentItem,
  itemType,
  allCandidateOptions,
  unmappedCandidateOptions,
  onSubmit,
  onClose,
}: ModalProps) {
  const noMappingValue = 'noMapping';

  const currentCandidate = !currentItem.candidateKey
    ? undefined
    : {
        key: currentItem.candidateKey,
        label: allCandidateOptions[currentItem.candidateKey].label,
      };

  const handleSubmit = ({ selectedCandidate }: FormValues) => {
    onSubmit([
      {
        sourceKey: currentItem.sourceKey,
        candidateKey:
          selectedCandidate !== noMappingValue ? selectedCandidate : undefined,
      },
    ]);
    onClose();
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

  return (
    <Modal open onExit={onClose} title={`Map existing ${itemType}`}>
      <h3>Current data set {itemType}</h3>
      <SummaryList>
        <SummaryListItem term="Name">{currentItem?.sourceKey}</SummaryListItem>
        <SummaryListItem term="Label">
          {currentItem?.source.label}
        </SummaryListItem>
      </SummaryList>
      <FormProvider
        initialValues={{ selectedCandidate: currentItem.candidateKey }}
        validationSchema={Yup.object({
          selectedCandidate: Yup.string().required(
            `Select the next data set ${itemType}`,
          ),
        })}
      >
        {({ formState }) => {
          return (
            <Form id="mapping-form" onSubmit={handleSubmit}>
              <FormFieldRadioSearchGroup<FormValues>
                alwaysShowOptions={[noMappingValue]}
                hint={`Choose a ${itemType} that will be mapped to the current data set ${itemType} (see above).`}
                legend={`Next data set ${itemType}`}
                name="selectedCandidate"
                options={options}
                order={[]}
                searchLabel={`Search ${itemType}s`}
                small
              />
              <ButtonGroup>
                <Button disabled={formState.isSubmitting} type="submit">
                  {`Update ${itemType} mapping`}
                </Button>
                <ButtonText disabled={formState.isSubmitting} onClick={onClose}>
                  Cancel
                </ButtonText>
              </ButtonGroup>
            </Form>
          );
        }}
      </FormProvider>
    </Modal>
  );
}

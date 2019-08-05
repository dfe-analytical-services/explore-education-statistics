import Link from '@admin/components/Link';
import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import Button from '@common/components/Button';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';

interface FormValues {
  publicationTitle: string;
  methodologyChoice?: 'existing' | 'new';
  selectedMethodologyId?: string;
  selectedContactId: string;
}

interface Props {
  contacts: ContactDetails[];
  methodologies: IdTitlePair[];
  values: FormValues;
  onEditHandler: () => void;
  onConfirmHandler: (values: FormValues) => void;
  onCancelHandler: () => void;
}

const CreatePublicationSummary = ({
  contacts,
  methodologies,
  values,
  onEditHandler,
  onConfirmHandler,
  onCancelHandler,
}: Props) => {
  const getSelectedMethodology = (selectedMethodologyId: string) =>
    methodologies.find(
      methodology => methodology.id === selectedMethodologyId,
    ) || methodologies[0];

  const getSelectedContact = (contactId: string) =>
    contacts.find(contact => contact.id === contactId) || contacts[0];

  return (
    <>
      <SummaryList>
        <SummaryListItem term="Publication title">
          {values.publicationTitle}
        </SummaryListItem>
        <SummaryListItem term="Methodology">
          {values.selectedMethodologyId
            ? getSelectedMethodology(values.selectedMethodologyId).title
            : 'Create new methodology'}
        </SummaryListItem>
        <SummaryListItem term="Contact">
          {getSelectedContact(values.selectedContactId).contactName}
        </SummaryListItem>
        <SummaryListItem
          term=""
          actions={
            <Link to="#" onClick={onEditHandler}>
              Edit publication details
            </Link>
          }
        />
      </SummaryList>
      <Button onClick={() => onConfirmHandler(values)}>
        Confirm and create new publication
      </Button>
      <div className="govuk-!-margin-top-6">
        <Link to="#" onClick={onCancelHandler}>
          Cancel publication
        </Link>
      </div>
    </>
  );
};

export default CreatePublicationSummary;

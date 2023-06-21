import PublicationContactForm, {
  PublicationContactFormValues,
} from '@admin/pages/publication/components/PublicationContactForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService from '@admin/services/publicationService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

const PublicationContactPage = () => {
  const { publication, onReload } = usePublicationContext();
  const [readOnly, toggleReadOnly] = useToggle(true);

  const {
    value: contact,
    setState: setContact,
  } = useAsyncHandledRetry(
    async () => publicationService.getContact(publication.id),
    [publication],
  );

  const handleSubmit = async (updatedContact: PublicationContactFormValues) => {
    if (!contact) {
      return;
    }
    const nextContact = await publicationService.updateContact(
      publication.id,
      updatedContact,
    );

    setContact({ value: nextContact });
    toggleReadOnly.on();
    onReload();
  };

  if (!contact) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Contact for this publication</h2>
      <div className="govuk-grid-row  govuk-!-margin-bottom-6">
        <div className="govuk-grid-column-three-quarters">
          <p>
            They will be the main point of contact for data and methodology
            enquiries for this publication and its releases.
          </p>
        </div>
      </div>
      {readOnly ? (
        <>
          <SummaryList>
            <SummaryListItem term="Team name">
              {contact.teamName}
            </SummaryListItem>
            <SummaryListItem term="Team email">
              {contact.teamEmail && (
                <a href={`mailto:${contact.teamEmail}`}>{contact.teamEmail}</a>
              )}
            </SummaryListItem>
            <SummaryListItem term="Contact name">
              {contact.contactName}
            </SummaryListItem>
            <SummaryListItem term="Contact telephone">
              {contact.contactTelNo}
            </SummaryListItem>
          </SummaryList>
          {publication.permissions.canUpdateContact && (
            <Button variant="secondary" onClick={toggleReadOnly.off}>
              Edit contact details
            </Button>
          )}
        </>
      ) : (
        <PublicationContactForm
          initialValues={contact}
          onCancel={toggleReadOnly.on}
          onSubmit={handleSubmit}
        />
      )}
    </>
  );
};

export default PublicationContactPage;

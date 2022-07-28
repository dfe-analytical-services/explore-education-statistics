import { useLastLocation } from '@admin/contexts/LastLocationContext';
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
import { useLocation } from 'react-router';

const PublicationContactPage = () => {
  const {
    publicationId,
    publication: contextPublication,
    onReload,
  } = usePublicationContext();
  const location = useLocation();
  const lastLocation = useLastLocation();
  const [readOnly, toggleReadOnly] = useToggle(true);

  const { value: publication } = useAsyncHandledRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? publicationService.getMyPublication(publicationId)
        : contextPublication,
    [publicationId],
  );

  const handleSubmit = async ({
    teamName,
    teamEmail,
    contactName,
    contactTelNo,
  }: PublicationContactFormValues) => {
    if (!publication) {
      return;
    }
    await publicationService.updatePublication(publicationId, {
      ...publication,
      contact: {
        teamName,
        teamEmail,
        contactName,
        contactTelNo,
      },
    });

    onReload();
  };

  if (!publication) {
    return <LoadingSpinner />;
  }

  const { contact } = publication;

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
          {publication.permissions.canUpdatePublication && (
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

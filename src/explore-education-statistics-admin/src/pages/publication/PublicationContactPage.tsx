import Button from '@common/components/Button';
import PublicationContactForm, {
  FormValues,
} from '@admin/pages/publication/components/PublicationContactForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService from '@admin/services/publicationService';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

const PublicationContactPage = () => {
  const { publication, onReload } = usePublicationContext();
  const { contact } = publication;
  const [readOnly, toggleReadOnly] = useToggle(true);

  const handleSubmit = async ({
    teamName,
    teamEmail,
    contactName,
    contactTelNo,
  }: FormValues) => {
    await publicationService.updatePublication(publication.id, {
      contact: {
        teamName,
        teamEmail,
        contactName,
        contactTelNo,
      },
      title: publication.title,
      topicId: publication.topicId,
    });

    onReload();
  };

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
              {contact?.teamName}
            </SummaryListItem>
            <SummaryListItem term="Team email">
              {contact?.teamEmail && (
                <a href={`mailto:${contact.teamEmail}`}>{contact.teamEmail}</a>
              )}
            </SummaryListItem>
            <SummaryListItem term="Contact name">
              {contact?.contactName}
            </SummaryListItem>
            <SummaryListItem term="Contact telephone">
              {contact?.contactTelNo}
            </SummaryListItem>
          </SummaryList>
          <Button variant="secondary" onClick={toggleReadOnly.off}>
            Edit contact details
          </Button>
        </>
      ) : (
        <PublicationContactForm
          initialValues={{
            teamName: contact?.teamName ?? '',
            teamEmail: contact?.teamEmail ?? '',
            contactName: contact?.contactName ?? '',
            contactTelNo: contact?.contactTelNo ?? '',
          }}
          onCancel={toggleReadOnly.on}
          onSubmit={handleSubmit}
        />
      )}
    </>
  );
};

export default PublicationContactPage;

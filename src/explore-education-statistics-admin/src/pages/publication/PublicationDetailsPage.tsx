import PublicationDetailsForm, {
  FormValues,
} from '@admin/pages/publication/components/PublicationDetailsForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService from '@admin/services/publicationService';
import themeService from '@admin/services/themeService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

const PublicationDetailsPage = () => {
  const { publication, onReload } = usePublicationContext();
  const {
    contact,
    id,
    permissions,
    supersededById,
    themeId,
    title,
    topicId,
  } = publication;
  const [readOnly, toggleReadOnly] = useToggle(true);

  const { value, isLoading } = useAsyncHandledRetry(async () => {
    const themes = await themeService.getThemes();
    if (!supersededById && !permissions.canUpdatePublicationSupersededBy) {
      return { themes };
    }

    const allPublications = await publicationService.getPublicationSummaries();
    const publications = allPublications.filter(pub => pub.id !== id);

    return {
      themes,
      publications,
    };
  });

  const { themes, publications } = value ?? {};

  const currentTheme = themes?.find(theme => theme.id === themeId);
  const currentTopic = currentTheme?.topics.find(topic => topic.id === topicId);
  const currentSupersededByPublication = publications?.find(
    pub => pub.id === supersededById,
  );

  const handleSubmit = async (values: FormValues) => {
    await publicationService.updatePublication(publication.id, {
      ...values,
      contact: {
        teamName: contact?.teamName ?? '',
        teamEmail: contact?.teamEmail ?? '',
        contactName: contact?.contactName ?? '',
        contactTelNo: contact?.contactTelNo ?? '',
      },
    });
    onReload();
  };

  return (
    <LoadingSpinner loading={isLoading}>
      {readOnly ? (
        <>
          <h2>Publication details</h2>
          <SummaryList>
            <SummaryListItem term="Publication title">{title}</SummaryListItem>
            <SummaryListItem term="Theme">
              {currentTheme?.title}
            </SummaryListItem>
            <SummaryListItem term="Topic">
              {currentTopic?.title}
            </SummaryListItem>
            <SummaryListItem term="Superseding publication">
              {supersededById
                ? currentSupersededByPublication?.title
                : 'This publication is not archived'}
            </SummaryListItem>
          </SummaryList>
          <Button variant="secondary" onClick={toggleReadOnly.off}>
            Edit publication details
          </Button>
        </>
      ) : (
        <PublicationDetailsForm
          canUpdatePublicationSupersededBy={
            permissions.canUpdatePublicationSupersededBy
          }
          canUpdatePublicationTitle={permissions.canUpdatePublicationTitle}
          initialValues={{
            supersededById,
            title,
            topicId,
          }}
          publications={publications}
          themes={themes}
          onCancel={toggleReadOnly.on}
          onSubmit={handleSubmit}
        />
      )}
    </LoadingSpinner>
  );
};

export default PublicationDetailsPage;

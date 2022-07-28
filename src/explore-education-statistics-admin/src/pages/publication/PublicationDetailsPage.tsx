import PublicationDetailsForm, {
  PublicationDetailsFormValues,
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
    const theme = await themeService.getTheme(themeId);
    const topic = theme?.topics.find(themeTopic => themeTopic.id === topicId);
    if (!supersededById) {
      return { theme, topic };
    }

    const supersedingPublication = await publicationService.getPublication(id);

    return {
      theme,
      topic,
      supersedingPublication,
    };
  });

  const { theme, topic, supersedingPublication } = value ?? {};

  const handleSubmit = async (values: PublicationDetailsFormValues) => {
    await publicationService.updatePublication(publication.id, {
      ...values,
      contact,
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
            <SummaryListItem term="Theme">{theme?.title}</SummaryListItem>
            <SummaryListItem term="Topic">{topic?.title}</SummaryListItem>
            <SummaryListItem term="Superseding publication">
              {supersededById
                ? supersedingPublication?.title
                : 'This publication is not archived'}
            </SummaryListItem>
          </SummaryList>
          {permissions.canUpdatePublication && (
            <Button variant="secondary" onClick={toggleReadOnly.off}>
              Edit publication details
            </Button>
          )}
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
          publicationId={id}
          onCancel={toggleReadOnly.on}
          onSubmit={handleSubmit}
        />
      )}
    </LoadingSpinner>
  );
};

export default PublicationDetailsPage;

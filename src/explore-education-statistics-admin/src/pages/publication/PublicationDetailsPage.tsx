import PublicationDetailsForm from '@admin/pages/publication/components/PublicationDetailsForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService from '@admin/services/publicationService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

const PublicationDetailsPage = () => {
  const { publication, onReload } = usePublicationContext();
  const { id, title, summary, permissions, supersededById, theme } =
    publication;
  const [readOnly, toggleReadOnly] = useToggle(true);

  const { value: supersedingPublication, isLoading } =
    useAsyncHandledRetry(async () => {
      if (!supersededById) {
        return undefined;
      }

      return publicationService.getPublication(supersededById);
    }, [supersededById]);

  return (
    <LoadingSpinner loading={isLoading}>
      {readOnly ? (
        <>
          <h2>Publication details</h2>
          <SummaryList>
            <SummaryListItem term="Publication title">{title}</SummaryListItem>
            <SummaryListItem term="Publication summary">
              {summary ?? 'Not set'}
            </SummaryListItem>
            <SummaryListItem term="Theme">{theme?.title}</SummaryListItem>
            <SummaryListItem term="Superseding publication">
              {supersededById
                ? supersedingPublication?.title
                : 'This publication is not archived'}
            </SummaryListItem>
          </SummaryList>
          {(permissions.canUpdatePublication ||
            permissions.canUpdatePublicationSummary) && (
            <Button variant="secondary" onClick={toggleReadOnly.off}>
              Edit publication details
            </Button>
          )}
        </>
      ) : (
        <PublicationDetailsForm
          canUpdatePublication={permissions.canUpdatePublication}
          canUpdatePublicationSummary={permissions.canUpdatePublicationSummary}
          initialValues={{
            supersededById,
            title,
            summary,
            themeId: theme.id,
          }}
          publicationId={id}
          publicationSlug={publication.slug}
          onCancel={toggleReadOnly.on}
          onSubmit={onReload}
        />
      )}
    </LoadingSpinner>
  );
};

export default PublicationDetailsPage;

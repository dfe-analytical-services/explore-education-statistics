import ButtonLink from '@admin/components/ButtonLink';
import Page from '@admin/components/Page';
import EducationInNumbersPagesTable from '@admin/pages/education-in-numbers/components/EducationInNumbersPagesTable';
import educationInNumbersQueries from '@admin/queries/educationInNumbersQueries';
import { educationInNumbersCreateRoute } from '@admin/routes/routes';
import educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { useQuery } from '@tanstack/react-query';
import React from 'react';

const EducationInNumbersListPage = () => {
  const [isReordering, toggleReordering] = useToggle(false);

  const {
    data: pages = [],
    isLoading,
    refetch: refetchPages,
  } = useQuery(educationInNumbersQueries.listLatestPages);

  if (isLoading) {
    return <LoadingSpinner loading={isLoading} />;
  }

  return (
    <Page
      title="Education in Numbers pages"
      breadcrumbs={[
        {
          name: 'Manage Education in Numbers',
        },
      ]}
    >
      {pages.length > 0 ? (
        <EducationInNumbersPagesTable
          einPages={pages}
          onDelete={async id => {
            educationInNumbersService.deleteEducationInNumbersPage(id);
            await refetchPages();
          }}
          isReordering={isReordering}
          onCancelReordering={toggleReordering.off}
          onConfirmReordering={async pageIds => {
            await educationInNumbersService.reorderEducationInNumbersPages(
              pageIds,
            );
            await refetchPages();
            toggleReordering.off();
          }}
        />
      ) : (
        <p className="govuk-!-margin-bottom-8">No pages created yet.</p>
      )}

      <ButtonGroup>
        <ButtonLink to={educationInNumbersCreateRoute.path}>
          Add new page
        </ButtonLink>
        {/* Only show reorder button if there are at least two *subpages* */}
        {pages.length > 2 && (
          <ModalConfirm
            confirmText="OK"
            title="Reorder pages"
            triggerButton={<Button variant="secondary">Reorder pages</Button>}
            onConfirm={toggleReordering.on}
          >
            <WarningMessage>
              All changes made to page order appear immediately on the public
              website.
            </WarningMessage>
          </ModalConfirm>
        )}
      </ButtonGroup>
    </Page>
  );
};

export function GetEducationInNumbersPageStatus(page: EinSummary) {
  if (page.published === undefined) {
    return page.version === 0 ? 'Draft' : 'Draft amendment';
  }
  return 'Published';
}

export default EducationInNumbersListPage;

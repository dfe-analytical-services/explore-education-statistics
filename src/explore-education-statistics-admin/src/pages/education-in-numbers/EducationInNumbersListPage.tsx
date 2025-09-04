import React from 'react';
import ButtonLink from '@admin/components/ButtonLink';
import Page from '@admin/components/Page';
import { educationInNumbersCreateRoute } from '@admin/routes/routes';
import ButtonGroup from '@common/components/ButtonGroup';
import { useQuery } from '@tanstack/react-query';
import educationInNumbersQueries from '@admin/queries/educationInNumbersQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { formatInTimeZone } from 'date-fns-tz';
import educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import { generatePath } from 'react-router';
import {
  EducationInNumbersRouteParams,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import { useHistory } from 'react-router-dom';
import ModalConfirm from '@common/components/ModalConfirm';
import ButtonText from '@common/components/ButtonText';
import Link from '@admin/components/Link';
import styles from './EducationInNumbersListPage.module.scss';

const EducationInNumbersListPage = () => {
  const history = useHistory();

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
        <table
          className={styles.table}
          data-testid="education-in-numbers-table"
        >
          <thead>
            <tr>
              <th scope="col">Title</th>
              <th scope="col">Slug</th>
              <th scope="col">Status</th>
              <th scope="col">Published</th>
              <th scope="col">Version</th>
              <th scope="col">Actions</th>
            </tr>
          </thead>

          <tbody>
            {pages.map(page => (
              <tr key={page.title}>
                <td data-testid="Title" className={styles.title}>
                  {page.title}
                </td>
                <td data-testid="Slug">{page.slug ?? 'N/A'}</td>
                <td data-testid="Status">
                  {GetEducationInNumbersPageStatus(page)}
                </td>
                <td data-testid="Published">
                  {page.published
                    ? formatInTimeZone(
                        page.published,
                        'Europe/London',
                        'HH:mm:ss - d MMMM yyyy',
                      )
                    : 'Not yet published'}
                </td>
                <td data-testid="Version">{page.version}</td>
                <td data-testid="Actions">
                  <ButtonGroup className={styles.actions}>
                    {page.published === undefined && page.version > 0 && (
                      <Link
                        to={`/education-in-numbers/${page.previousVersionId}/summary`}
                      >
                        View currently published page
                      </Link>
                    )}
                    <Link to={`/education-in-numbers/${page.id}/summary`}>
                      {page.published === undefined ? 'Edit' : 'View'}
                    </Link>
                    {page.published !== undefined && (
                      <ButtonText
                        onClick={async () => {
                          const newPage =
                            await educationInNumbersService.createEducationInNumbersPageAmendment(
                              page.id,
                            );
                          history.push(
                            generatePath<EducationInNumbersRouteParams>(
                              educationInNumbersSummaryRoute.path,
                              {
                                educationInNumbersPageId: newPage.id,
                              },
                            ),
                          );
                        }}
                      >
                        Create amendment
                      </ButtonText>
                    )}
                    {page.published === undefined &&
                      page.slug !== undefined && ( // prevent removal of root Education in numbers page
                        <ModalConfirm
                          title={
                            page.version === 0
                              ? `Are you sure you want to delete ${page.title}?`
                              : `Are you sure you want to cancel the amendment to ${page.title}?`
                          }
                          triggerButton={
                            <ButtonText>
                              {page.version === 0
                                ? 'Delete'
                                : 'Cancel amendment'}
                            </ButtonText>
                          }
                          onConfirm={async () => {
                            await educationInNumbersService.deleteEducationInNumbersPage(
                              page.id,
                            );

                            await refetchPages();
                          }}
                        >
                          <p>
                            {page.version === 0
                              ? "This will remove updates you've made to this page."
                              : "By cancelling this amendment, you will lose all changes you've made to the amendment. The latest published version of this page will remain live and be unaffected."}
                          </p>
                        </ModalConfirm>
                      )}
                  </ButtonGroup>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p className="govuk-!-margin-bottom-8">No pages created yet.</p>
      )}

      <ButtonLink to={educationInNumbersCreateRoute.path}>
        Add new page
      </ButtonLink>
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

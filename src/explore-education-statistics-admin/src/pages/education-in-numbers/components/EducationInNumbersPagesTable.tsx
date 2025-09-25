import { GetEducationInNumbersPageStatus } from '@admin/pages/education-in-numbers/EducationInNumbersListPage';
import styles from '@admin/pages/education-in-numbers/components/EducationInNumbersPagesTable.module.scss';
import {
  EducationInNumbersRouteParams,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersService, {
  EinSummaryWithPrevVersion,
} from '@admin/services/educationInNumbersService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import ReorderableList from '@common/components/ReorderableList';
import reorder from '@common/utils/reorder';
import { formatInTimeZone } from 'date-fns-tz';
import React, { useEffect, useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import { Link } from 'react-router-dom';

interface Props {
  pages: EinSummaryWithPrevVersion[];
  isReordering: boolean;
  onCancelReordering: () => void;
  onConfirmReordering: (pageIds: string[]) => void;
  onDelete: (id: string) => Promise<void> | void;
}

const EducationInNumbersPagesTable = ({
  pages: initialPage,
  isReordering,
  onCancelReordering,
  onConfirmReordering,
  onDelete,
}: Props) => {
  const history = useHistory();
  const [pages, setPages] = useState(initialPage);

  useEffect(() => {
    setPages(initialPage);
  }, [initialPage]);

  if (isReordering) {
    return (
      <ReorderableList
        heading="Reorder pages"
        id="pages"
        list={pages.map(page => ({
          id: page.id,
          label: page.title,
        }))}
        onCancel={() => {
          setPages(initialPage);
          onCancelReordering();
        }}
        onConfirm={() => onConfirmReordering(pages.map(p => p.id))}
        onMoveItem={({ prevIndex, nextIndex }) => {
          const reordered = reorder(pages, prevIndex, nextIndex);
          setPages(reordered);
        }}
      />
    );
  }
  return (
    <table className={styles.table} data-testid="education-in-numbers-table">
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
                          {page.version === 0 ? 'Delete' : 'Cancel amendment'}
                        </ButtonText>
                      }
                      onConfirm={async () => {
                        await onDelete(page.id);
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
  );
};

export default EducationInNumbersPagesTable;

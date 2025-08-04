import React from 'react';
import ButtonLink from '@admin/components/ButtonLink';
import Page from '@admin/components/Page';
import { educationInNumbersCreateRoute } from '@admin/routes/routes';
import ButtonGroup from '@common/components/ButtonGroup';
import styles from './EducationInNumbersListPage.module.scss';

const EducationInNumbersListPage = () => {
  return (
    <Page
      title="Education in Numbers pages"
      breadcrumbs={[
        {
          name: 'Manage Education in Numbers',
        },
      ]}
    >
      {'TODO - use fetched pages'.length > 0 ? (
        <table
          className={styles.table}
          data-testid="education-in-numbers-table"
        >
          <thead>
            <tr>
              <th scope="col">Title</th>
              <th scope="col">Slug</th>
              <th scope="col">Published</th>
              <th scope="col">Updated</th>
              <th scope="col">Actions</th>
            </tr>
          </thead>

          <tbody>
            {[
              { title: 'Overview', slug: '', id: 'abc' },
              { title: 'Key Statistics', slug: 'key-statistics', id: 'xyz' },
            ].map(page => (
              <tr key={page.title}>
                <td data-testid="Title" className={styles.title}>
                  {page.title}
                </td>
                <td data-testid="Slug">{page.slug}</td>
                <td data-testid="Published">published</td>
                <td data-testid="Updated">updated</td>
                <td data-testid="Actions">
                  <ButtonGroup className={styles.actions}>
                    <ButtonLink to={`/education-in-numbers/${page.id}/summary`}>
                      Edit
                    </ButtonLink>
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

export default EducationInNumbersListPage;

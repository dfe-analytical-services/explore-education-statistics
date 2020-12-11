import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import importStatusService from '@admin/services/importStatusService';

const BauImportsPage = () => {
  const { value, isLoading } = useAsyncRetry(() =>
    importStatusService.getAllIncompleteImports(),
  );

  return (
    <Page
      title="Incomplete imports"
      caption="Manage incomplete imports"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Incomplete imports' },
      ]}
    >
      <LoadingSpinner loading={isLoading} text="Loading imports">
        <table>
          <thead>
            <tr>
              {/* EES-1655 */}
              {/* <th scope="col" className="govuk-table__header">
              Subject title
            </th> */}
              <th scope="col">Subject ID</th>
              <th scope="col">Data file name</th>
              <th scope="col">Data file rows</th>
              <th scope="col">Status</th>
              <th scope="col">Stage Percentage Complete</th>
              <th scope="col">Go to release data files</th>
            </tr>
          </thead>
          {value && (
            <tbody>
              {value.map(subject => (
                <tr key={subject.subjectId}>
                  {/* EES-1655 */}
                  {/* <th scope="row" className="govuk-table__header">
                    {subject.subjectTitle}
                  </th> */}
                  <td>{subject.subjectId}</td>
                  <td>{subject.dataFileName}</td>
                  <td>{subject.numberOfRows}</td>
                  <td>{subject.status}</td>
                  <td>{subject.stagePercentageComplete}</td>
                  <td>
                    <Link
                      to={`/publication/${subject.publicationId}/release/${subject.releaseId}/data`}
                    >
                      {subject.publicationTitle} {subject.releaseTitle}
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          )}
        </table>
      </LoadingSpinner>
    </Page>
  );
};

export default BauImportsPage;

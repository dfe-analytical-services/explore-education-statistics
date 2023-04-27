import React from 'react';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
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
              <th scope="col">Status</th>
              <th scope="col">Subject Id</th>
              <th scope="col">Data filename</th>
              <th scope="col">Rows</th>
              <th scope="col">Batches</th>
              <th scope="col">Stage complete</th>
              <th scope="col">Overall complete</th>
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
                  <td>{subject.status}</td>
                  <td>{subject.subjectId}</td>
                  <td>{subject.dataFileName}</td>
                  <td>{subject.totalRows ?? 'Unknown'}</td>
                  <td>{subject.batches}</td>
                  <td>{subject.stagePercentageComplete}%</td>
                  <td>{subject.percentageComplete}%</td>
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

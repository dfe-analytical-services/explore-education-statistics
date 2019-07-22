import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import React from 'react';

function PermalinkPage() {
  return (
    <Page
      title="{Permalink title}"
      caption="Permanent data table"
      breadcrumbs={[
        { name: 'Data tables', link: '/table-tool' },
        { name: 'Permanent link', link: '/table-tool' },
      ]}
    >
      <table className="govuk-table">
        <caption className="govuk-heading-s">Example table</caption>
        <thead className="govuk-table__head">
          <tr>
            <th className="govuk-table__header" />
            <th
              className="govuk-table__header govuk-table__cell--numeric"
              scope="col"
            >
              2012/13
            </th>
            <th
              className="govuk-table__header govuk-table__cell--numeric"
              scope="col"
            >
              2013/14
            </th>
            <th
              className="govuk-table__header govuk-table__cell--numeric"
              scope="col"
            >
              2014/15
            </th>
            <th
              className="govuk-table__header govuk-table__cell--numeric"
              scope="col"
            >
              2015/16
            </th>
            <th
              className="govuk-table__header govuk-table__cell--numeric"
              scope="col"
            >
              2016/17
            </th>
          </tr>
        </thead>
        <tbody className="govuk-table__body">
          <tr className="govuk-table__row ">
            <th className="govuk-table__header">Unauthorised absence rate</th>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              1.1
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              1.1
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              1.1
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              1.1
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              1.3
            </td>
          </tr>
          <tr className="govuk-table__row ">
            <th className="govuk-table__header">Overall absence rate</th>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              5.3
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.5
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.6
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.6
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.7
            </td>
          </tr>
          <tr className="govuk-table__row ">
            <th className="govuk-table__header">Authorised absence rate</th>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.2
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              3.5
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              3.5
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              3.4
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              3.4
            </td>
          </tr>
        </tbody>
      </table>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Create your own tables online
      </h2>
      <p>
        Use our tool to build tables using our range of national and regional
        data.
      </p>
      <ButtonLink prefetch as="/table-tool/" href="/table-tool">
        Create tables
      </ButtonLink>

      <PrintThisPage
        analytics={{
          category: 'Page print',
          action: 'Print this page link selected',
        }}
      />
    </Page>
  );
}

export default PermalinkPage;

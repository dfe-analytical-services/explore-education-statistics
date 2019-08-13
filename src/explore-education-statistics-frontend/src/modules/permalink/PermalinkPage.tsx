import FormattedDate from '@common/components/FormattedDate';
import permalinkService, { Permalink } from '@common/services/permalinkService';
import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import { NextContext } from 'next';
import React, { Component } from 'react';

interface Props {
  permalink: string;
  data: Permalink;
}

class PermalinkPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    permalink: string;
  }>) {
    const { permalink } = query;

    const request = permalinkService.getPermalink(permalink);

    const data = await request;

    return {
      data,
      permalink,
    };
  }

  public render() {
    const { data } = this.props;
    return (
      <Page
        title={data.title}
        caption="Permanent data table"
        breadcrumbs={[
          { name: 'Data tables', link: '/data-tables' },
          { name: 'Permanent link', link: '/data-tables' },
        ]}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <dl className="dfe-meta-content govuk-!-margin-bottom-9">
              <dt className="govuk-caption-m">Created:</dt>
              <dd data-testid="created-date">
                <strong>
                  {' '}
                  <FormattedDate>{data.created}</FormattedDate>{' '}
                </strong>
              </dd>
            </dl>
          </div>
          <div className="govuk-grid-column-one-third">
            {/* <RelatedAside>
              <h3>Related content</h3>
            </RelatedAside> */}
          </div>
        </div>

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
        <p className="govuk-body-s">Source: DfE prototype example statistics</p>
        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Create your own tables online
        </h2>
        <p>
          Use our tool to build tables using our range of national and regional
          data.
        </p>
        <ButtonLink prefetch as="/data-tables/" href="/data-tables">
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
}

export default PermalinkPage;

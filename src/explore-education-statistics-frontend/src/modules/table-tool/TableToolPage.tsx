import tableBuilderService, {
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import ButtonText from '@common/components/ButtonText';
import LinkContainer from '@common/components/LinkContainer';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { NextContext } from 'next';
import React, { Component, createRef } from 'react';
import permalinkService from '@common/modules/full-table/services/permalinkService';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableTool from '@common/modules/table-tool/components/TableTool';

export interface PublicationOptions {
  id: string;
  title: string;
  topics: {
    id: string;
    title: string;
    publications: {
      id: string;
      title: string;
      slug: string;
    }[];
  }[];
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId: string;
}

interface State {
  permalinkId: string;
  permalinkLoading: boolean;
}

class TableToolPage extends Component<Props, State> {
  public state: State = {
    permalinkId: '',
    permalinkLoading: false,
  };

  private dataTableRef = createRef<HTMLTableElement>();

  public static async getInitialProps({ query }: NextContext) {
    const themeMeta = await tableBuilderService.getThemes();

    const publication = themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.slug === query.publicationSlug);

    return {
      themeMeta,
      publicationId: publication ? publication.id : '',
    };
  }

  private handlePermalinkClick = async ({
    query,
    tableHeaders,
  }: {
    query: TableDataQuery;
    tableHeaders: TableHeadersFormValues;
  }) => {
    this.setState({ permalinkLoading: true });

    const { id: permalinkId } = await permalinkService.createTablePermalink({
      ...query,
      configuration: {
        tableHeadersConfig: tableHeaders,
      },
    });

    this.setState({
      permalinkId,
      permalinkLoading: false,
    });

    // Router.push(`${window.location.pathname}/permalink/${permalinkId}`);
  };

  public render() {
    const { themeMeta, publicationId } = this.props;
    const { permalinkId, permalinkLoading } = this.state;

    return (
      <Page title="Create your own tables online" caption="Table Tool" wide>
        <p>
          Choose the data and area of interest you want to explore and then use
          filters to create your table.
        </p>

        <p>
          Once you've created your table, you can download the data it contains
          for your own offline analysis.
        </p>

        <TableTool
          publicationId={publicationId}
          themeMeta={themeMeta}
          finalStepExtra={finalStepProps => (
            <>
              <h3>Share your table</h3>
              <ul className="govuk-list">
                <li>
                  {permalinkId ? (
                    <>
                      <div>Generated permanent link:</div>
                      <LinkContainer
                        url={`${window.location.host}/data-tables/permalink/${permalinkId}`}
                      />
                      <div>
                        <a
                          className="govuk-link"
                          href={`/data-tables/permalink/${permalinkId}`}
                          title="View created table permalink"
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          View permanent link
                        </a>
                      </div>
                    </>
                  ) : (
                    <>
                      {permalinkLoading ? (
                        <>
                          Generating permanent link
                          <LoadingSpinner inline size={19} />
                        </>
                      ) : (
                        <ButtonText
                          disabled={permalinkLoading}
                          onClick={() =>
                            this.handlePermalinkClick(finalStepProps)
                          }
                        >
                          Generate permanent link
                        </ButtonText>
                      )}
                    </>
                  )}
                </li>
              </ul>

              <h3>Additional options</h3>

              <ul className="govuk-list">
                <li>
                  <Link
                    as={`/find-statistics/${finalStepProps.publication.slug}`}
                    to={`/find-statistics/publication?publication=${finalStepProps.publication.slug}`}
                  >
                    Go to publication
                  </Link>
                </li>
                <li>
                  <DownloadCsvButton
                    publicationSlug={finalStepProps.publication.slug}
                    fullTable={finalStepProps.createdTable}
                  />
                </li>

                <li>
                  <a href="#api">Access developer API</a>
                </li>
                <li>
                  <Link
                    as={`/methodology/${finalStepProps.publication.slug}`}
                    to={`/methodology/methodology?methodology=${finalStepProps.publication.slug}`}
                  >
                    Go to methodology
                  </Link>
                </li>
              </ul>
              <p className="govuk-body">
                If you have a question about the data or methods used to create
                this table contact the named statistician via the relevant
                release page.
              </p>
            </>
          )}
        />
      </Page>
    );
  }
}

export default TableToolPage;

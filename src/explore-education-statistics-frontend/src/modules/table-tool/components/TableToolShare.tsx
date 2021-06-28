import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import UrlContainer from '@common/components/UrlContainer';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import permalinkService from '@common/services/permalinkService';
import {
  SelectedPublication,
  TableDataQuery,
} from '@common/services/tableBuilderService';
import Link from '@frontend/components/Link';
import React, { useEffect, useState } from 'react';

interface Props {
  currentTableHeaders?: TableHeadersConfig;
  query: TableDataQuery;
  selectedPublication: SelectedPublication;
}

const TableToolShare = ({
  currentTableHeaders,
  query,
  selectedPublication,
}: Props) => {
  const [permalinkId, setPermalinkId] = useState<string>('');
  const [permalinkLoading, setPermalinkLoading] = useState<boolean>(false);

  useEffect(() => {
    setPermalinkId('');
  }, [currentTableHeaders]);

  const handlePermalinkClick = async () => {
    if (!currentTableHeaders) {
      return;
    }
    setPermalinkLoading(true);

    const { id } = await permalinkService.createPermalink(
      {
        query,
        configuration: {
          tableHeaders: mapUnmappedTableHeaders(currentTableHeaders),
        },
      },
      selectedPublication.selectedRelease.id,
    );

    setPermalinkId(id);
    setPermalinkLoading(false);
  };

  const handleCopyClick = () => {
    const el = document.querySelector(
      "[data-testid='permalink-generated-url']",
    ) as HTMLInputElement;
    el?.select();
    document.execCommand('copy');
  };

  return (
    <>
      {permalinkId ? (
        <div className="dfe-align--left">
          <h3 className="govuk-heading-s">Generated share link</h3>

          <div className="govuk-inset-text">
            Use the link below to see a version of this page that you can
            bookmark for future reference, or copy the link to send on to
            somebody else to view.
          </div>

          <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-2">
            <UrlContainer
              data-testid="permalink-generated-url"
              url={`${window.location.origin}/data-tables/permalink/${permalinkId}`}
            />
          </p>

          <button
            type="button"
            className="govuk-button govuk-button--secondary govuk-!-margin-right-3"
            onClick={handleCopyClick}
          >
            Copy link
          </button>

          <Link
            className="govuk-!-margin-top-0 govuk-button"
            to="/data-tables/permalink/[permalink]"
            as={`/data-tables/permalink/${permalinkId}`}
            title="View created table permalink"
            target="_blank"
            rel="noopener noreferrer"
          >
            View share link
          </Link>
        </div>
      ) : (
        <>
          <h3 className="govuk-heading-s">Save table</h3>
          <LoadingSpinner
            alert
            inline
            loading={permalinkLoading}
            size="sm"
            text="Generating permanent link"
          >
            <ButtonText onClick={handlePermalinkClick}>
              Generate shareable link
            </ButtonText>
          </LoadingSpinner>
        </>
      )}
    </>
  );
};

export default TableToolShare;

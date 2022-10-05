import ButtonText from '@common/components/ButtonText';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import UrlContainer from '@common/components/UrlContainer';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import permalinkService from '@common/services/permalinkService';
import {
  SelectedPublication,
  TableDataQuery,
} from '@common/services/tableBuilderService';
import ButtonLink from '@frontend/components/ButtonLink';
import React, { useEffect, useState } from 'react';

const linkInstructions =
  'Use the link below to see a version of this page that you can bookmark for future reference, or copy the link to send on to somebody else to view.';

interface Props {
  tableHeaders?: TableHeadersConfig;
  query: TableDataQuery;
  selectedPublication: SelectedPublication;
}

const TableToolShare = ({
  tableHeaders,
  query,
  selectedPublication,
}: Props) => {
  const [permalinkUrl, setPermalinkUrl] = useState('');
  const [permalinkLoading, setPermalinkLoading] = useState<boolean>(false);
  const [screenReaderMessage, setScreenReaderMessage] = useState('');

  useEffect(() => {
    setPermalinkUrl('');
  }, [tableHeaders]);

  const handlePermalinkClick = async () => {
    if (!tableHeaders) {
      return;
    }
    setPermalinkLoading(true);

    const { id } = await permalinkService.createPermalink(
      {
        query,
        configuration: {
          tableHeaders: mapUnmappedTableHeaders(tableHeaders),
        },
      },
      selectedPublication.selectedRelease.id,
    );

    setPermalinkUrl(`${window.location.origin}/data-tables/permalink/${id}`);
    setPermalinkLoading(false);

    setScreenReaderMessage(`Shareable link generated. ${linkInstructions}`);
  };

  const handleCopyClick = () => {
    navigator.clipboard.writeText(permalinkUrl);
    setScreenReaderMessage('Link copied to the clipboard.');
  };

  return (
    <>
      {!permalinkUrl ? (
        <>
          <h3 className="govuk-heading-s">Save table</h3>
          <LoadingSpinner
            alert
            inline
            loading={permalinkLoading}
            size="sm"
            text="Generating shareable link"
          >
            <ButtonText onClick={handlePermalinkClick}>
              Generate shareable link
            </ButtonText>
          </LoadingSpinner>
        </>
      ) : (
        <>
          <h3 className="govuk-heading-s">Generated share link</h3>

          <div className="govuk-inset-text" aria-hidden>
            {linkInstructions}
          </div>

          <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-2">
            <UrlContainer
              data-testid="permalink-generated-url"
              url={permalinkUrl}
            />
          </p>

          <ButtonGroup>
            <Button variant="secondary" onClick={handleCopyClick}>
              Copy link
            </Button>

            <ButtonLink to={permalinkUrl}>View share link</ButtonLink>
          </ButtonGroup>
        </>
      )}

      <ScreenReaderMessage message={screenReaderMessage} />
    </>
  );
};

export default TableToolShare;

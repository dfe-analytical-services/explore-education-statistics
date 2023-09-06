import ButtonText from '@common/components/ButtonText';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import UrlContainer from '@common/components/UrlContainer';
import useToggle from '@common/hooks/useToggle';
import ErrorMessage from '@common/components/ErrorMessage';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import logger from '@common/services/logger';
import permalinkSnapshotService from '@common/services/permalinkSnapshotService';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import ButtonLink from '@frontend/components/ButtonLink';
import React, { useEffect, useState } from 'react';

const linkInstructions =
  'Use the link below to see a version of this page that you can bookmark for future reference, or copy the link to send on to somebody else to view.';

interface Props {
  tableHeaders?: TableHeadersConfig;
  query: ReleaseTableDataQuery;
}

const TableToolShare = ({ tableHeaders, query }: Props) => {
  const [permalinkUrl, setPermalinkUrl] = useState('');
  const [permalinkLoading, setPermalinkLoading] = useState<boolean>(false);
  const [screenReaderMessage, setScreenReaderMessage] = useState('');
  const [permalinkError, togglePermalinkError] = useToggle(false);

  useEffect(() => {
    setPermalinkUrl('');
  }, [tableHeaders]);

  const handlePermalinkClick = async () => {
    if (!tableHeaders) {
      return;
    }
    setPermalinkLoading(true);

    try {
      const { id } = await permalinkSnapshotService.createPermalink({
        query,
        configuration: {
          tableHeaders: mapUnmappedTableHeaders(tableHeaders),
        },
      });

      setPermalinkUrl(`${process.env.PUBLIC_URL}data-tables/permalink/${id}`);
      setPermalinkLoading(false);

      setScreenReaderMessage(`Shareable link generated. ${linkInstructions}`);
    } catch (err) {
      logger.error(err);
      togglePermalinkError();
      setScreenReaderMessage(
        `Error: There was a problem generating the share link.`,
      );
      setPermalinkLoading(false);
    }
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
            {permalinkError ? (
              <ErrorMessage>
                There was a problem generating the share link.
              </ErrorMessage>
            ) : (
              <ButtonText onClick={handlePermalinkClick}>
                Generate shareable link
              </ButtonText>
            )}
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

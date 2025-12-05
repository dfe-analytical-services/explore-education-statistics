import Details from '@common/components/Details';
import ContentHtml from '@common/components/ContentHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { DataSetDataGuidance } from '@common/services/releaseDataGuidanceService';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { ReactNode, useMemo } from 'react';
import styles from './ReleaseDataGuidanceDataFile.module.scss';

interface Props {
  dataSet: DataSetDataGuidance;
  renderContent?: (dataSet: DataSetDataGuidance) => ReactNode;
}

const ReleaseDataGuidanceDataFile = ({ dataSet, renderContent }: Props) => {
  const { filename, variables, footnotes, name } = dataSet;

  const geographicLevels = useMemo(
    () => dataSet.geographicLevels.sort().join('; '),
    [dataSet.geographicLevels],
  );

  const timePeriod = useMemo(() => {
    const { from, to } = dataSet.timePeriods;

    if (from && to) {
      return from === to ? from : `${from} to ${to}`;
    }

    return from || to;
  }, [dataSet.timePeriods]);

  const contentItem = useMemo(() => {
    if (typeof renderContent === 'function') {
      return (
        <SummaryListItem term="Content">
          {renderContent(dataSet)}
        </SummaryListItem>
      );
    }

    if (!dataSet.content) {
      return null;
    }

    return (
      <SummaryListItem term="Content">
        <ContentHtml html={dataSet.content} testId="fileGuidanceContent" />
      </SummaryListItem>
    );
  }, [renderContent, dataSet]);

  return (
    <>
      <SummaryList className="govuk-!-margin-bottom-6">
        <SummaryListItem term="Filename">{filename}</SummaryListItem>
        {geographicLevels && (
          <SummaryListItem term="Geographic levels">
            {geographicLevels}
          </SummaryListItem>
        )}
        {timePeriod && (
          <SummaryListItem term="Time period">{timePeriod}</SummaryListItem>
        )}
        {contentItem}
      </SummaryList>

      {variables.length > 0 && (
        <Details
          summary="Variable names and descriptions"
          summaryAfter={<VisuallyHidden>{` for ${name}`}</VisuallyHidden>}
          className="govuk-!-margin-bottom-4"
        >
          <p>
            Variable names and descriptions for this file are provided below:
          </p>

          <table data-testid="Variables">
            <thead>
              <tr>
                <th scope="col">Variable name</th>
                <th scope="col">Variable description</th>
              </tr>
            </thead>
            <tbody>
              {variables.map(({ value, label }, index) => (
                // eslint-disable-next-line react/no-array-index-key
                <tr key={index}>
                  <td className={styles.tableOverflowWrap}>{value}</td>
                  <td>{label}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </Details>
      )}

      {footnotes.length > 0 && (
        <Details
          summary="Footnotes"
          summaryAfter={<VisuallyHidden>{` for ${name}`}</VisuallyHidden>}
        >
          <ol data-testid="Footnotes">
            {footnotes.map(footnote => (
              <li key={footnote.id}>
                <ContentHtml html={footnote.label} />
              </li>
            ))}
          </ol>
        </Details>
      )}
    </>
  );
};

export default ReleaseDataGuidanceDataFile;

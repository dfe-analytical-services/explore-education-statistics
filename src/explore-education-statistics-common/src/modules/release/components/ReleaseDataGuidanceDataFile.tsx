import Details from '@common/components/Details';
import ContentHtml from '@common/components/ContentHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { SubjectDataGuidance } from '@common/services/releaseDataGuidanceService';
import React, { ReactNode, useMemo } from 'react';
import styles from './ReleaseDataGuidanceDataFile.module.scss';

interface Props {
  subject: SubjectDataGuidance;
  renderContent?: (subject: SubjectDataGuidance) => ReactNode;
}

const ReleaseDataGuidanceDataFile = ({ subject, renderContent }: Props) => {
  const { filename, variables, footnotes } = subject;

  const geographicLevels = useMemo(
    () => subject.geographicLevels.sort().join('; '),
    [subject.geographicLevels],
  );

  const timePeriod = useMemo(() => {
    const { from, to } = subject.timePeriods;

    if (from && to) {
      return from === to ? from : `${from} to ${to}`;
    }

    return from || to;
  }, [subject.timePeriods]);

  const contentItem = useMemo(() => {
    if (typeof renderContent === 'function') {
      return (
        <SummaryListItem term="Content">
          {renderContent(subject)}
        </SummaryListItem>
      );
    }

    if (!subject.content) {
      return null;
    }

    return (
      <SummaryListItem term="Content">
        <ContentHtml html={subject.content} testId="fileGuidanceContent" />
      </SummaryListItem>
    );
  }, [renderContent, subject]);

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
        <Details summary="Footnotes">
          <ol data-testid="Footnotes">
            {footnotes.map(footnote => (
              <li key={footnote.id}>{footnote.label}</li>
            ))}
          </ol>
        </Details>
      )}
    </>
  );
};

export default ReleaseDataGuidanceDataFile;

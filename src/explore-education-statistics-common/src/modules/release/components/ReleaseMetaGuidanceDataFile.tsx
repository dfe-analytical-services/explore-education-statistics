import Details from '@common/components/Details';
import SanitizeHtml from '@common/components/SanitizeHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { SubjectMetaGuidance } from '@common/services/releaseMetaGuidanceService';
import React, { ReactNode, useMemo } from 'react';

interface Props {
  subject: SubjectMetaGuidance;
  renderContent?: (subject: SubjectMetaGuidance) => ReactNode;
}

const ReleaseMetaGuidanceDataFile = ({ subject, renderContent }: Props) => {
  const { filename, variables } = subject;

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
        <SanitizeHtml
          dirtyHtml={subject.content}
          testId="fileGuidanceContent"
        />
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

      <Details summary="Variable names and descriptions">
        <p>Variable names and descriptions for this file are provided below:</p>

        <table>
          <thead>
            <tr>
              <th scope="col">Variable name</th>
              <th scope="col">Variable description</th>
            </tr>
          </thead>
          <tbody>
            {variables.map(({ value, label }) => (
              <tr key={value}>
                <td>{value}</td>
                <td>{label}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Details>
    </>
  );
};

export default ReleaseMetaGuidanceDataFile;

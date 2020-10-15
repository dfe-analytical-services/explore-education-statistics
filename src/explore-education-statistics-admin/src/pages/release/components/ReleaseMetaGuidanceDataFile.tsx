import { SubjectMetaGuidance } from '@admin/services/releaseMetaGuidanceService';
import Details from '@common/components/Details';
import SanitizeHtml from '@common/components/SanitizeHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { ReactNode } from 'react';

interface Props {
  subject: SubjectMetaGuidance;
  renderContent?: (subject: SubjectMetaGuidance) => ReactNode;
}

const ReleaseMetaGuidanceDataFile = ({ subject, renderContent }: Props) => {
  const { filename, timePeriods, geographicLevels, variables } = subject;

  return (
    <>
      <SummaryList className="govuk-!-margin-bottom-6">
        <SummaryListItem term="Filename">{filename}</SummaryListItem>
        <SummaryListItem term="Geographic levels">
          {geographicLevels.join('; ')}
        </SummaryListItem>
        <SummaryListItem term="Time period">
          {`${timePeriods.from} to ${timePeriods.to}`}
        </SummaryListItem>
        <SummaryListItem term="Content">
          {typeof renderContent === 'function' ? (
            renderContent(subject)
          ) : (
            <SanitizeHtml
              dirtyHtml={subject.content}
              testId="fileGuidanceContent"
            />
          )}
        </SummaryListItem>
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
